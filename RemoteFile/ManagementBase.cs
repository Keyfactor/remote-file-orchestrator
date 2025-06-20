﻿// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class ManagementBase : RemoteFileJobTypeBase, IManagementJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.RemoteFile.Management";

        internal RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer(config.CertificateStoreDetails.Properties);

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);

                SetJobProperties(config, config.CertificateStoreDetails, logger);

                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, UserName, UserPassword, config.CertificateStoreDetails.StorePath, StorePassword, FileTransferProtocol, SSHPort, IncludePortInSPN);
                certificateStore.Initialize(SudoImpersonatedUser);

                PathFile storePathFile = RemoteCertificateStore.SplitStorePathFile(config.CertificateStoreDetails.StorePath);

                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        logger.LogDebug($"BEGIN add Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            if (ApplicationSettings.CreateStoreIfMissing)
                                CreateStore(certificateStoreSerializer, config);
                            else
                                throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        certificateStore.LoadCertificateStore(certificateStoreSerializer, false);
                        certificateStore.AddCertificate((config.JobCertificate.Alias ?? new X509Certificate2(Convert.FromBase64String(config.JobCertificate.Contents), config.JobCertificate.PrivateKeyPassword, X509KeyStorageFlags.EphemeralKeySet).Thumbprint), config.JobCertificate.Contents, config.Overwrite, config.JobCertificate.PrivateKeyPassword, RemoveRootCertificate);
                        certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, StorePassword, certificateStore.RemoteHandler));

                        logger.LogDebug($"END add Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        break;

                    case CertStoreOperationType.Remove:
                        logger.LogDebug($"BEGIN Delete Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        else
                        {
                            certificateStore.LoadCertificateStore(certificateStoreSerializer, false);
                            certificateStore.DeleteCertificateByAlias(config.JobCertificate.Alias);
                            certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, StorePassword, certificateStore.RemoteHandler));
                        }
                        logger.LogDebug($"END Delete Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        break;

                    case CertStoreOperationType.Create:
                        logger.LogDebug($"BEGIN create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (certificateStore.DoesStoreExist())
                        {
                            logger.LogWarning($"Certificate store {config.CertificateStoreDetails.StorePath} already exists.");
                            return new JobResult() { Result = OrchestratorJobStatusJobResult.Warning, JobHistoryId = config.JobHistoryId, FailureMessage = $"Certificate store {config.CertificateStoreDetails.StorePath} already exists.  Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}" };
                        }
                        else
                        {
                            CreateStore(certificateStoreSerializer, config);
                        }
                        logger.LogDebug($"END create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        break;

                    default:
                        return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}" };
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception for {config.Capability}: {RemoteFileException.FlattenExceptionMessages(ex, string.Empty)} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") };
            }
            finally
            {
                if (certificateStore.RemoteHandler != null)
                    certificateStore.Terminate();
            }

            logger.LogDebug($"...End {config.Capability} job for job id {config.JobId}");
            return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }

        private void CreateStore(ICertificateStoreSerializer certificateStoreSerializer, ManagementJobConfiguration config)
        {
            dynamic properties = JsonConvert.DeserializeObject(config.CertificateStoreDetails.Properties.ToString());
            string linuxFilePermissions = properties.LinuxFilePermissionsOnStoreCreation == null || string.IsNullOrEmpty(properties.LinuxFilePermissionsOnStoreCreation.Value) ?
                ApplicationSettings.DefaultLinuxPermissionsOnStoreCreation :
                properties.LinuxFilePermissionsOnStoreCreation.Value;

            string linuxFileOwner = properties.LinuxFileOwnerOnStoreCreation == null || string.IsNullOrEmpty(properties.LinuxFileOwnerOnStoreCreation.Value) ?
                ApplicationSettings.DefaultOwnerOnStoreCreation :
                properties.LinuxFileOwnerOnStoreCreation.Value;

            certificateStore.CreateCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.StorePath, linuxFilePermissions, linuxFileOwner);
        }
    }
}
