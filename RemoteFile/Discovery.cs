// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public class Discovery: IDiscoveryJobExtension
    {
        public string ExtensionName => "";

        public JobResult ProcessJob(DiscoveryJobConfiguration config, SubmitDiscoveryUpdate submitDiscovery)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: { config.ClientMachine }");
            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }

            string[] directoriesToSearch = config.JobProperties["dirs"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] extensionsToSearch = config.JobProperties["extensions"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ignoredDirs = config.JobProperties["ignoreddirs"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] filesTosearch = config.JobProperties["patterns"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool includeSymLinks = Convert.ToBoolean(config.JobProperties["symLinks"]);

            List<string> locations = new List<string>();

            RemoteCertificateStore certificateStore = new RemoteCertificateStore(config.ClientMachine, config.ServerUsername, config.ServerPassword, directoriesToSearch[0].Substring(0, 1) == "/" ? RemoteCertificateStore.ServerTypeEnum.Linux : RemoteCertificateStore.ServerTypeEnum.Windows);

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);

                if (directoriesToSearch.Length == 0)
                    throw new RemoteFileException("Blank or missing search directories for Discovery.");
                if (extensionsToSearch.Length == 0)
                    throw new RemoteFileException("Blank or missing search extensions for Discovery.");
                if (filesTosearch.Length == 0)
                    filesTosearch = new string[] { "*" };

                locations = certificateStore.FindStores(directoriesToSearch, extensionsToSearch, filesTosearch, includeSymLinks);
                foreach (string ignoredDir in ignoredDirs)
                    locations = locations.Where(p => !p.StartsWith(ignoredDir)).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception for {config.Capability}: {RemoteFileException.FlattenExceptionMessages(ex, string.Empty)} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Server {config.ClientMachine}:") };
            }
            finally
            {
                certificateStore.Terminate();
            }

            try
            {
                logger.LogDebug($"Stores returned for {config.Capability}:");
                foreach (string location in locations)
                {
                    logger.LogDebug($"    {location}");
                }
                submitDiscovery.Invoke(locations);
                logger.LogDebug($"...End {config.Capability} job for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                string errorMessage = RemoteFileException.FlattenExceptionMessages(ex, string.Empty);
                logger.LogError($"Exception returning store locations for {config.Capability}: {errorMessage} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Server {config.ClientMachine}: {errorMessage}" };
            }
        }
    }
}