// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class ManagementBase : RemoteFileJobTypeBase, IManagementJobExtension
    {
        static Mutex mutex = new Mutex(false, "ModifyStore");

        public string ExtensionName => "";

        internal RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: { config.CertificateStoreDetails.ClientMachine }");
            logger.LogDebug($"Store Path: { config.CertificateStoreDetails.StorePath }");
            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties == null ? new Dictionary<string, object>() : config.JobProperties)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer();

            try
            {
                mutex.WaitOne();

                string userName = PAMUtilities.ResolvePAMField(_resolver, logger, "Server User Name", config.ServerUsername);
                string userPassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Server Password", config.ServerPassword);
                string storePassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Store Password", config.CertificateStoreDetails.StorePassword);

                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, userName, userPassword, config.CertificateStoreDetails.StorePath, storePassword, config.JobProperties);
                certificateStore.Initialize();

                PathFile storePathFile = RemoteCertificateStore.SplitStorePathFile(config.CertificateStoreDetails.StorePath);

                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        logger.LogDebug($"BEGIN create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            if (ApplicationSettings.CreateStoreIfMissing)
                                CreateStore(config);
                            else
                                throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        certificateStore.LoadCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.Properties);
                        certificateStore.AddCertificate((config.JobCertificate.Alias ?? new X509Certificate2(Convert.FromBase64String(config.JobCertificate.Contents), config.JobCertificate.PrivateKeyPassword).Thumbprint), config.JobCertificate.Contents, config.Overwrite, config.JobCertificate.PrivateKeyPassword);
                        certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, storePassword, config.CertificateStoreDetails.Properties, certificateStore.RemoteHandler));

                        logger.LogDebug($"END create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        break;

                    case CertStoreOperationType.Remove:
                        logger.LogDebug($"BEGIN Delete Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (!certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                        }
                        else
                        {
                            certificateStore.LoadCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.Properties);
                            certificateStore.DeleteCertificateByAlias(config.JobCertificate.Alias);
                            certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, storePassword, config.CertificateStoreDetails.Properties, certificateStore.RemoteHandler));
                        }
                        logger.LogDebug($"END Delete Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        break;

                    case CertStoreOperationType.Create:
                        logger.LogDebug($"BEGIN create Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
                        if (certificateStore.DoesStoreExist())
                        {
                            throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} already exists.");
                        }
                        else
                        {
                            CreateStore(config);
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
                mutex.ReleaseMutex();

                if (certificateStore.RemoteHandler != null)
                    certificateStore.Terminate();
            }

            logger.LogDebug($"...End {config.Capability} job for job id {config.JobId}");
            return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }

        private void CreateStore(ManagementJobConfiguration config)
        {
            dynamic properties = JsonConvert.DeserializeObject(config.CertificateStoreDetails.Properties.ToString());
            string linuxFilePermissions = properties.LinuxFilePermissionsOnStoreCreation == null || string.IsNullOrEmpty(properties.LinuxFilePermissionsOnStoreCreation.Value) ?
                ApplicationSettings.DefaultLinuxPermissionsOnStoreCreation :
                properties.LinuxFilePermissionsOnStoreCreation.Value;

            certificateStore.CreateCertificateStore(config.CertificateStoreDetails.StorePath, linuxFilePermissions);
        }
    }
}
