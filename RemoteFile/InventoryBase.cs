using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Logging;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class InventoryBase : IInventoryJobExtension
    {
        protected ILogger logger;

        public string ExtensionName => string.Empty;

        RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: { config.CertificateStoreDetails.ClientMachine }");
            logger.LogDebug($"Store Path: { config.CertificateStoreDetails.StorePath }");
            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties == null ? new Dictionary<string,object>() : config.JobProperties)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer();
            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                ApplicationSettings.Initialize(this.GetType().Assembly.Location);
                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, config.ServerUsername, config.ServerPassword, config.CertificateStoreDetails.StorePath, config.CertificateStoreDetails.StorePassword, config.JobProperties);
                certificateStore.LoadCertificateStore();

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
                logger.LogDebug($"Exception for {config.Capability}: {RemoteFileException.FlattenExceptionMessages(ex, string.Empty)} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") };
            }
            finally
            {
                if (certificateStore.SSH != null)
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
                logger.LogDebug($"Exception returning certificates for {config.Capability}: {errorMessage} for job id {config.JobId}");
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = RemoteFileException.FlattenExceptionMessages(ex, $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}:") };
            }
        }

        internal abstract ICertificateStoreSerializer GetCertificateStoreSerializer();
    }
}
