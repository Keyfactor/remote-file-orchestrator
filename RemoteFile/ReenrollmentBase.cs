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
using static Org.BouncyCastle.Math.EC.ECCurve;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.Linq;

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

        public JobResult ProcessJob(ReenrollmentJobConfiguration config, SubmitReenrollmentCSR submitReenrollmentUpdate)
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
                    properties.CreateCSROnDevice.Value;

                string keyType = !config.JobProperties.ContainsKey("keyType") || config.JobProperties["keyType"] == null || string.IsNullOrEmpty(config.JobProperties["keyType"].ToString()) ? string.Empty : config.JobProperties["keyType"].ToString();
                int? keySize = !config.JobProperties.ContainsKey("keySize") || config.JobProperties["keySize"] == null || string.IsNullOrEmpty(config.JobProperties["keySize"].ToString()) ? null : Convert.ToInt32(config.JobProperties["keySize"]);
                string subjectText = !config.JobProperties.ContainsKey("subjectText") || config.JobProperties["subjectText"] == null || config.JobProperties["subjectText"] == null || string.IsNullOrEmpty(config.JobProperties["subjectText"].ToString()) ? string.Empty : config.JobProperties["subjectText"].ToString();
                string sans = !config.JobProperties.ContainsKey("SANs") || config.JobProperties["SANs"] == null || string.IsNullOrEmpty(config.JobProperties["SANs"].ToString()) ? string.Empty : config.JobProperties["SANs"].ToString();

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

                certificateStore.LoadCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.Properties, false);
                if (createCSROnDevice)
                {
                    throw new Exception("Not implemented");
                }
                else
                {
                    string csr = GenerateCSR
                }
                certificateStore.AddCertificate((config.JobCertificate.Alias ?? new X509Certificate2(Convert.FromBase64String(config.JobCertificate.Contents), config.JobCertificate.PrivateKeyPassword, X509KeyStorageFlags.EphemeralKeySet).Thumbprint), config.JobCertificate.Contents, config.Overwrite, config.JobCertificate.PrivateKeyPassword);
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

        private string GenerateCSR(string subjectText, SupportedKeyTypeEnum keyType, int keySize, List<string> sans)
        {
            //Code logic to:
            //  1) Generate a new CSR
            //  2) Include the provided subject text
            //  3) Include the list of SANs
            //  3) Include the OID corresponding to a Time Stamping request, so Command recognizes this as a request for re-enrollment
            //  4) Return the base64 encoded CSR.

            // this approach relies on the Bouncy Castle Crypto package, and not the Microsoft x509 certificate libraries.

            IAsymmetricCipherKeyPairGenerator keyPairGenerator = null;
            switch (keyType)
            {
                case SupportedKeyTypeEnum.RSA:
                    keyPairGenerator = new RsaKeyPairGenerator();
                    break;
                case SupportedKeyTypeEnum.ECC:
                    keyPairGenerator = new ECKeyPairGenerator();
                    break;
            }

            var keyGenParams = new KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(new CryptoApiRandomGenerator()), keySize);
            keyPairGenerator.Init(keyGenParams);

            var keyPair = keyPairGenerator.GenerateKeyPair();
            var subject = new X509Name(subjectText);

            // Add SAN entries
            var subAltNameList = new List<GeneralName>();
            sans.ForEach(san => subAltNameList.Add(new GeneralName(GeneralName.DnsName, san.Trim())));
            var generalSubAltNames = new GeneralNames(subAltNameList.ToArray());

            // Create Key Usage attribute
            int keyUsage = KeyUsage.DigitalSignature | KeyUsage.NonRepudiation;
            var keyUsageExtension = new KeyUsage(keyUsage);

            // Add Extended Key Usage extension for re-enrollment (1.3.6.1.5.5.7.3.8 is the OID for time stamping, the Command CA should be configured to recognize a CSR with this OID as a request for re-enrollment)
            //var timestampOid = new DerObjectIdentifier("1.3.6.1.5.5.7.3.8"); // https://oidref.com/1.3.6.1.5.5.7.3.8
            //var extendedKeyUsage = new ExtendedKeyUsage(new DerObjectIdentifier[] { timestampOid });

            // Create extensions
            var extensionsGenerator = new X509ExtensionsGenerator();
            extensionsGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, generalSubAltNames);
            extensionsGenerator.AddExtension(X509Extensions.KeyUsage, true, keyUsageExtension);
            //extensionsGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, extendedKeyUsage);
            X509Extensions extensions = extensionsGenerator.Generate();

            // Create attribute set with extensions
            var attributeSet = new AttributePkcs(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensions));

            // Include the attributes in the request
            var csr = new Pkcs10CertificationRequest(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest.Id, subject, keyPair.Public, new DerSet(attributeSet), keyPair.Private);

            // encode the CSR as base64
            var encodedCsr = Convert.ToBase64String(csr.GetEncoded());
            return encodedCsr;
        }
    }
}
