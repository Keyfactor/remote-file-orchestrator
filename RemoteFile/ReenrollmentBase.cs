// Copyright 2021 Keyfactor
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
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.Security.Cryptography;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class ReenrollmentBase : RemoteFileJobTypeBase, IReenrollmentJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.RemoteFile";

        internal RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        internal enum SupportedKeyTypeEnum
        {
            RSA,
            ECC
        }

        public JobResult ProcessJob(ReenrollmentJobConfiguration config, SubmitReenrollmentCSR submitReenrollment)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: {config.CertificateStoreDetails.ClientMachine}");
            logger.LogDebug($"Store Path: {config.CertificateStoreDetails.StorePath}");

            logger.LogDebug($"Job Properties:");
            foreach (KeyValuePair<string, object> keyValue in config.JobProperties == null ? new Dictionary<string, object>() : config.JobProperties)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }
            
            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer(config.CertificateStoreDetails.Properties);

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
                bool createCSROnDevice = properties.CreateCSROnDevice == null || string.IsNullOrEmpty(properties.CreateCSROnDevice.Value) ?
                    ApplicationSettings.CreateCSROnDevice :
                    Convert.ToBoolean(properties.CreateCSROnDevice.Value);

                string keyType = !config.JobProperties.ContainsKey("keyType") || config.JobProperties["keyType"] == null || string.IsNullOrEmpty(config.JobProperties["keyType"].ToString()) ? string.Empty : config.JobProperties["keyType"].ToString();
                int keySize = !config.JobProperties.ContainsKey("keySize") || config.JobProperties["keySize"] == null || string.IsNullOrEmpty(config.JobProperties["keySize"].ToString()) ? 2048 : Convert.ToInt32(config.JobProperties["keySize"]);
                string subjectText = !config.JobProperties.ContainsKey("subjectText") || config.JobProperties["subjectText"] == null || config.JobProperties["subjectText"] == null || string.IsNullOrEmpty(config.JobProperties["subjectText"].ToString()) ? string.Empty : config.JobProperties["subjectText"].ToString();

                //TODO - Set SANs, Alias and Overwrite "for real" once product figures out how to pass that
                string alias = "abcd";
                string sans = "reenroll2.Keyfactor.com&reenroll1.keyfactor.com&reenroll3.Keyfactor.com";
                bool overwrite = true;

                // validate parameters
                string keyTypes = string.Join(",", Enum.GetNames(typeof(SupportedKeyTypeEnum)));
                if (!Enum.TryParse(keyType.ToUpper(), out SupportedKeyTypeEnum keyTypeEnum))
                {
                    throw new RemoteFileException($"Unsupported KeyType value {keyType}.  Supported types are {keyTypes}.");
                }

                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, userName, userPassword, config.CertificateStoreDetails.StorePath, storePassword, config.JobProperties);
                certificateStore.Initialize(sudoImpersonatedUser);

                PathFile storePathFile = RemoteCertificateStore.SplitStorePathFile(config.CertificateStoreDetails.StorePath);

                if (!certificateStore.DoesStoreExist())
                {
                    throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                }

                // generate CSR and call back to enroll certificate
                string csr = string.Empty;
                string pemPrivateKey = string.Empty;
                if (createCSROnDevice)
                {
                    csr = certificateStore.GenerateCSROnDevice(subjectText, keyTypeEnum, keySize, new List<string>(sans.Split('&', StringSplitOptions.RemoveEmptyEntries)), out pemPrivateKey);
                }
                else
                {
                    csr = certificateStore.GenerateCSR(subjectText, keyTypeEnum, keySize, new List<string>(sans.Split('&', StringSplitOptions.RemoveEmptyEntries)));
                }

                X509Certificate2 cert = submitReenrollment.Invoke(csr);

                if (!string.IsNullOrEmpty(pemPrivateKey))
                {
                    RSA rsa = RSA.Create();
                    rsa.ImportEncryptedPkcs8PrivateKey(string.Empty, Convert.FromBase64String(pemPrivateKey), out _);
                    cert = cert.CopyWithPrivateKey(rsa);
                }

                // save certificate
                certificateStore.LoadCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.Properties, false);
                certificateStore.AddCertificate((alias ?? cert.Thumbprint), Convert.ToBase64String(cert.Export(X509ContentType.Cert)), overwrite, null);
                certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, storePassword, certificateStore.RemoteHandler));

                logger.LogDebug($"END add Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
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
    }
