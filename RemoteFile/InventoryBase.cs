// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using Keyfactor.PKI.Extensions;
using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class InventoryBase : RemoteFileJobTypeBase, IInventoryJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.RemoteFile.Inventory";

        RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer(config.CertificateStoreDetails.Properties);
            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                SetJobProperties(config, config.CertificateStoreDetails, logger);

                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, UserName, UserPassword, config.CertificateStoreDetails.StorePath, StorePassword, FileTransferProtocol, SSHPort, IncludePortInSPN);
                certificateStore.Initialize(SudoImpersonatedUser);
                certificateStore.LoadCertificateStore(certificateStoreSerializer, true);

                List<List<X509CertificateExt>> collections = certificateStore.GetCertificateChains();

                logger.LogDebug($"Format returned certificates BEGIN");
                foreach (List<X509CertificateExt> collection in collections)
                {
                    if (collection.Count == 0)
                        continue;

                    X509CertificateExt issuedCertificate = collection[0];

                    List<string> certChain = new List<string>();
                    foreach (X509CertificateExt certificate in collection)
                    {
                        certChain.Add(Convert.ToBase64String(certificate.GetEncoded()));
                        logger.LogDebug(Convert.ToBase64String(certificate.GetEncoded()));
                    }

                    inventoryItems.Add(new CurrentInventoryItem()
                    {
                        ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                        Alias = string.IsNullOrEmpty(issuedCertificate.FriendlyNameExt) ? issuedCertificate.Thumbprint() : issuedCertificate.FriendlyNameExt,
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
                return new JobResult 
                    { 
                        Result = OrchestratorJobStatusJobResult.Failure, 
                        JobHistoryId = config.JobHistoryId, 
                        FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") 
                    };
            }
            finally
            {
                if (certificateStore.RemoteHandler != null)
                    certificateStore.Terminate();
            }

            try
            {
                submitInventory.Invoke(inventoryItems);
                logger.LogDebug("...End {ConfigCapability} job for job id {ConfigJobId}", config.Capability, config.JobId);
                var jobResultStatus = OrchestratorJobStatusJobResult.Success;
                var jobMsg = string.Empty;
                if (certificateStore.RemoteHandler != null && certificateStore.RemoteHandler.Warnings.Length > 0)
                {
                    jobResultStatus = OrchestratorJobStatusJobResult.Warning;
                    jobMsg = certificateStore.RemoteHandler.Warnings.Aggregate(jobMsg, (current, warning) => current + (warning + Environment.NewLine));
                }
                return new JobResult
                {
                    Result = jobResultStatus, 
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = jobMsg
                };
            }
            catch (Exception ex)
            {
                string errorMessage = RemoteFileException.FlattenExceptionMessages(ex, string.Empty);
                logger.LogError($"Exception returning certificates for {config.Capability}: {errorMessage} for job id {config.JobId}");
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure, 
                    JobHistoryId = config.JobHistoryId, 
                    FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:")
                };
            }
        }
    }
}
