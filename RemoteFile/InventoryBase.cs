﻿// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class InventoryBase : RemoteFileJobTypeBase, IInventoryJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.RemoteFile.Inventory";

        RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: { config.CertificateStoreDetails.ClientMachine }");
            logger.LogDebug($"Store Path: { config.CertificateStoreDetails.StorePath }");
            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties ?? new Dictionary<string,object>())
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer(config.CertificateStoreDetails.Properties);
            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                string userName = PAMUtilities.ResolvePAMField(_resolver, logger, "Server User Name", config.ServerUsername);
                string userPassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Server Password", config.ServerPassword);
                string storePassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Store Password", config.CertificateStoreDetails.StorePassword);

                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                dynamic properties = JsonConvert.DeserializeObject(config.CertificateStoreDetails.Properties.ToString());
                string sudoImpersonatedUser = properties.SudoImpersonatedUser == null || string.IsNullOrEmpty(properties.SudoImpersonatedUser.Value) ?
                    ApplicationSettings.DefaultSudoImpersonatedUser :
                    properties.SudoImpersonatedUser.Value;
                bool includePortInSPN = properties.IncludePortInSPN == null || string.IsNullOrEmpty(properties.IncludePortInSPN.Value) ?
                    false :
                    Convert.ToBoolean(properties.IncludePortInSPN.Value);

                ApplicationSettings.FileTransferProtocolEnum fileTransferProtocol = ApplicationSettings.FileTransferProtocol;
                if (properties.FileTransferProtocol != null && !string.IsNullOrEmpty(properties.FileTransferProtocol.Value))
                {
                    Enum.TryParse(properties.FileTransferProtocol.Value, out fileTransferProtocol);
                }

                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, userName, userPassword, config.CertificateStoreDetails.StorePath, storePassword, fileTransferProtocol, includePortInSPN);
                certificateStore.Initialize(sudoImpersonatedUser);
                certificateStore.LoadCertificateStore(certificateStoreSerializer, true);

                List<X509Certificate2Collection> collections = certificateStore.GetCertificateChains();

                logger.LogDebug($"Format returned certificates BEGIN");
                foreach (X509Certificate2Collection collection in collections)
                {
                    if (collection.Count == 0)
                        continue;

                    X509Certificate2Ext issuedCertificate = (X509Certificate2Ext)collection[0];

                    List<string> certChain = new List<string>();
                    foreach (X509Certificate2 certificate in collection)
                    {
                        certChain.Add(Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
                        logger.LogDebug(Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
                    }

                    inventoryItems.Add(new CurrentInventoryItem()
                    {
                        ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                        Alias = string.IsNullOrEmpty(issuedCertificate.FriendlyNameExt) ? issuedCertificate.Thumbprint : issuedCertificate.FriendlyNameExt,
                        PrivateKeyEntry = issuedCertificate.HasPrivateKey,
                        UseChainLevel = collection.Count > 1,
                        Certificates = certChain.ToArray()
                    });
                }
                logger.LogDebug($"Format returned certificates END");
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

            try
            {
                submitInventory.Invoke(inventoryItems);
                logger.LogDebug($"...End {config.Capability} job for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                string errorMessage = RemoteFileException.FlattenExceptionMessages(ex, string.Empty);
                logger.LogError($"Exception returning certificates for {config.Capability}: {errorMessage} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") };
            }
        }
    }
}
