using System;
using System.Collections.Generic;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;

using Org.BouncyCastle.Pkcs;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class InventoryBase : IInventoryJobExtension
    {
        protected ILogger logger;

        public string ExtensionName => string.Empty;

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");

            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                RemoteCertificateStore certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, config.ServerUsername, config.ServerPassword, config.CertificateStoreDetails.StorePath, config.CertificateStoreDetails.StorePassword, config.JobProperties);

                submitInventory.Invoke(inventoryItems);
                return new JobResult()
                {
                    JobHistoryId = config.JobHistoryId,
                    Result = certificates.Count == 0 ? OrchestratorJobStatusJobResult.Warning : OrchestratorJobStatusJobResult.Success,
                    FailureMessage = certificates.Count == 0 ? $"Certificate store {config.CertificateStoreDetails.StorePath} is empty." : string.Empty
                };
            }
            catch (Exception ex)
            {
                return new JobResult()
                {
                    JobHistoryId = config.JobHistoryId,
                    Result = OrchestratorJobStatusJobResult.Failure,
                    FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Certificate store {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:")
                };
            }

        }

        internal abstract Pkcs12Store GetRemoteCertificateStore(InventoryJobConfiguration config);
    }
}
