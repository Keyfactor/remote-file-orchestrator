// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.IO;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class ManagementBase : IManagementJobExtension
    {
        public string ExtensionName => "";

        internal RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} Management-{Enum.GetName(typeof(CertStoreOperationType), config.OperationType)} job for job id {config.JobId}...");

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                certificateStore = GetRemoteCertificateStore(config);

                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        logger.LogDebug($"Begin Create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        else
                        {
                            certificateStore.AddCertificate(config.JobCertificate.Alias, config.JobCertificate.Contents, config.Overwrite, config.JobCertificate.PrivateKeyPassword);
                            SaveRemoteCertificateStore(certificateStore);
                        }
                        break;

                    case CertStoreOperationType.Remove:
                        logger.LogDebug($"Begin Delete Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        else
                        {
                            certificateStore.DeleteCertificateByAlias(config.JobCertificate.Alias);
                            SaveRemoteCertificateStore(certificateStore);
                        }
                        break;

                    case CertStoreOperationType.Create:
                        logger.LogDebug($"Begin Create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} already exists.");
                        }
                        else
                        {
                            dynamic properties = JsonConvert.DeserializeObject(config.CertificateStoreDetails.Properties.ToString());
                            string linuxFilePermissions = properties.linuxFilePermissionsOnStoreCreation == null || string.IsNullOrEmpty(properties.linuxFilePermissionsOnStoreCreation.Value) ? 
                                ApplicationSettings.DefaultLinuxPermissionsOnStoreCreation :
                                properties.linuxFilePermissionsOnStoreCreation.Value;

                            certificateStore.CreateCertificateStore(config.CertificateStoreDetails.StorePath, linuxFilePermissions);
                        }
                        break;

                    default:
                        return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}" };
                }
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") };
            }
            finally
            {
                certificateStore.Terminate();
            }

            return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }

        internal abstract RemoteCertificateStore GetRemoteCertificateStore(ManagementJobConfiguration config);

        internal abstract void SaveRemoteCertificateStore(RemoteCertificateStore certificateStore);
    }
}