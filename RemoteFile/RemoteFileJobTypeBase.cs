// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class RemoteFileJobTypeBase
    {
        public IPAMSecretResolver _resolver;
        internal abstract ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties);

        internal string UserName { get; set; }
        internal string UserPassword { get; set; }
        internal string StorePassword { get; set; }
        internal string SudoImpersonatedUser { get; set; }
        internal bool RemoveRootCertificate { get; set; }
        internal int SSHPort { get; set; }
        internal bool IncludePortInSPN { get; set; }
        internal ApplicationSettings.FileTransferProtocolEnum FileTransferProtocol { get; set; }
        internal bool CreateCSROnDevice { get; set; }
        internal string KeyType { get; set; }
        internal int KeySize { get; set; }
        internal string SubjectText { get; set; }


        internal void SetJobProperties(JobConfiguration config, CertificateStore certificateStoreDetails, ILogger logger)
        {
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: {certificateStoreDetails.ClientMachine}");
            logger.LogDebug($"Store Path: {certificateStoreDetails.StorePath}");
            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties == null ? new Dictionary<string, object>() : config.JobProperties)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }

            UserName = PAMUtilities.ResolvePAMField(_resolver, logger, "Server User Name", config.ServerUsername);
            UserPassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Server Password", config.ServerPassword);
            StorePassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Store Password", certificateStoreDetails.StorePassword);

            dynamic properties = JsonConvert.DeserializeObject(certificateStoreDetails.Properties.ToString());

            SudoImpersonatedUser = properties.SudoImpersonatedUser == null || string.IsNullOrEmpty(properties.SudoImpersonatedUser.Value) ?
                ApplicationSettings.DefaultSudoImpersonatedUser :
                properties.SudoImpersonatedUser.Value;

            SSHPort = properties.SSHPort == null || string.IsNullOrEmpty(properties.SSHPort.Value) || !int.TryParse(properties.SSHPort.Value, out int notUsed) ?
                ApplicationSettings.SSHPort :
                properties.SSHPort;

            RemoveRootCertificate = properties.RemoveRootCertificate == null || string.IsNullOrEmpty(properties.RemoveRootCertificate.Value) ?
                false :
                Convert.ToBoolean(properties.RemoveRootCertificate.Value);

            IncludePortInSPN = properties.IncludePortInSPN == null || string.IsNullOrEmpty(properties.IncludePortInSPN.Value) ?
                false :
                Convert.ToBoolean(properties.IncludePortInSPN.Value);

            CreateCSROnDevice = properties.CreateCSROnDevice == null || string.IsNullOrEmpty(properties.CreateCSROnDevice.Value) ?
                ApplicationSettings.CreateCSROnDevice :
                Convert.ToBoolean(properties.CreateCSROnDevice.Value);

            FileTransferProtocol = ApplicationSettings.FileTransferProtocol;
            if (properties.FileTransferProtocol != null && !string.IsNullOrEmpty(properties.FileTransferProtocol.Value))
            {
                ApplicationSettings.FileTransferProtocolEnum fileTransferProtocol;
                if (Enum.TryParse(properties.FileTransferProtocol.Value, out fileTransferProtocol))
                    FileTransferProtocol = fileTransferProtocol;
            }

            if (config.JobProperties != null)
            {
                KeyType = !config.JobProperties.ContainsKey("keyType") || config.JobProperties["keyType"] == null || string.IsNullOrEmpty(config.JobProperties["keyType"].ToString()) ? string.Empty : config.JobProperties["keyType"].ToString();
                KeySize = !config.JobProperties.ContainsKey("keySize") || config.JobProperties["keySize"] == null || string.IsNullOrEmpty(config.JobProperties["keySize"].ToString()) || !int.TryParse(config.JobProperties["keySize"].ToString(), out int notUsed2) ? 2048 : Convert.ToInt32(config.JobProperties["keySize"]);
                SubjectText = !config.JobProperties.ContainsKey("subjectText") || config.JobProperties["subjectText"] == null || string.IsNullOrEmpty(config.JobProperties["subjectText"].ToString()) ? string.Empty : config.JobProperties["subjectText"].ToString();
            }        
        }
    }
}
