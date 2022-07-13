// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class DiscoveryBase: IDiscoveryJobExtension
    {
        public string ExtensionName => "";

        public JobResult ProcessJob(DiscoveryJobConfiguration config, SubmitDiscoveryUpdate submitDiscovery)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} job for job id {config.JobId}...");

            string[] directoriesToSearch = config.JobProperties["dirs"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] extensionsToSearch = config.JobProperties["extensions"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ignoredDirs = config.JobProperties["ignoreddirs"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] filesTosearch = config.JobProperties["patterns"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> locations = new List<string>();

            RemoteCertificateStore certificateStore = new RemoteCertificateStore();

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                certificateStore = GetRemoteCertificateStore(config);

                if (directoriesToSearch.Length == 0)
                    throw new RemoteFileException("Blank or missing search directories for Discovery.");
                if (extensionsToSearch.Length == 0)
                    throw new RemoteFileException("Blank or missing search extensions for Discovery.");
                if (filesTosearch.Length == 0)
                    filesTosearch = new string[] { "*" };

                locations = certificateStore.FindStores(directoriesToSearch, extensionsToSearch, filesTosearch);
                foreach (string ignoredDir in ignoredDirs)
                    locations = locations.Where(p => !p.StartsWith(ignoredDir)).ToList();
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Server {config.ClientMachine}:") };
            }
            finally
            {
                certificateStore.Terminate();
            }

            try
            {
                submitDiscovery.Invoke(locations);
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Server {config.ClientMachine}:") };
            }
        }

        internal abstract RemoteCertificateStore GetRemoteCertificateStore(DiscoveryJobConfiguration config);
    }
}