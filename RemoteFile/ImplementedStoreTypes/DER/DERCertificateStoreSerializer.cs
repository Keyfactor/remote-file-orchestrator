// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using Keyfactor.Logging;
using Keyfactor.PKI.PrivateKeys;
using Keyfactor.PKI.X509;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.DER
{
    class DERCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private string SeparatePrivateKeyFilePath { get; set; }

        private ILogger logger;

        public DERCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            LoadCustomProperties(storeProperties);
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool includePrivateKey)
        {
            logger.MethodEntry(LogLevel.Debug);

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            X509CertificateEntry certificate = GetCertificate(storeContentBytes);

            if (String.IsNullOrEmpty(SeparatePrivateKeyFilePath))
            {
                store.SetCertificateEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificate.Certificate).ToX509Certificate2().Thumbprint, certificate);
            }
            else
            {
                AsymmetricKeyEntry keyEntry = GetPrivateKey(storePassword ?? string.Empty, remoteHandler);
                store.SetKeyEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificate.Certificate).ToX509Certificate2().Thumbprint, keyEntry, new X509CertificateEntry[] { certificate });
            }

            // Second Pkcs12Store necessary because of an obscure BC bug where creating a Pkcs12Store without .Load (code above using "Set" methods only) does not set all internal hashtables necessary to avoid an error later
            //  when processing store.
            MemoryStream ms = new MemoryStream();
            store.Save(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
            ms.Position = 0;

            Pkcs12Store newStore = storeBuilder.Build();
            newStore.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());

            logger.MethodExit(LogLevel.Debug);
            return newStore;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            string alias = string.Empty;

            byte[] certificateBytes = new byte[] { };
            byte[] privateKeyBytes = new byte[] { };

            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            if (certificateStore.Aliases.Count() != 0)
            {

                if (certificateStore.Aliases.Count() > 1)
                    throw new RemoteFileException($"Cannot add a new certificate to a DER certificate store that already contains a certificate.");

                foreach (string currentAlias in certificateStore.Aliases)
                {
                    alias = currentAlias;
                    if (!certificateStore.IsKeyEntry(alias) && !string.IsNullOrEmpty(SeparatePrivateKeyFilePath))
                        throw new RemoteFileException($"DER certificate store has a private key at {SeparatePrivateKeyFilePath}, but no private key was passed with the certificate to this job.");
                }

                CertificateConverter certConverter = CertificateConverterFactory.FromBouncyCastleCertificate(certificateStore.GetCertificate(alias).Certificate);
                certificateBytes = certConverter.ToDER(string.IsNullOrEmpty(storePassword) ? string.Empty : storePassword);

                if (!string.IsNullOrEmpty(SeparatePrivateKeyFilePath))
                {
                    AsymmetricKeyParameter privateKey = certificateStore.GetKey(alias).Key;
                    X509Certificate certificate = certificateStore.GetCertificate(alias).Certificate;
                    AsymmetricKeyParameter publicKey = certificate.GetPublicKey();
                    PrivateKeyConverter keyConverter = PrivateKeyConverterFactory.FromBCKeyPair(privateKey, publicKey, false);

                    privateKeyBytes = string.IsNullOrEmpty(storePassword) ? keyConverter.ToPkcs8BlobUnencrypted() : keyConverter.ToPkcs8Blob(storePassword);
                }
            }

            storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath+storeFileName, Contents = certificateBytes });
            if (!string.IsNullOrEmpty(SeparatePrivateKeyFilePath))
                storeInfo.Add(new SerializedStoreInfo() { FilePath = SeparatePrivateKeyFilePath, Contents = privateKeyBytes });

            logger.MethodExit(LogLevel.Debug);

            return storeInfo;
        }

        public string GetPrivateKeyPath()
        {
            return SeparatePrivateKeyFilePath;
        }

        private void LoadCustomProperties(string storeProperties)
        {
            logger.MethodEntry(LogLevel.Debug);

            dynamic properties = JsonConvert.DeserializeObject(storeProperties);
            SeparatePrivateKeyFilePath = properties.SeparatePrivateKeyFilePath == null || string.IsNullOrEmpty(properties.SeparatePrivateKeyFilePath.Value) ? String.Empty : properties.SeparatePrivateKeyFilePath.Value;

            logger.MethodExit(LogLevel.Debug);
        }

        private X509CertificateEntry GetCertificate(byte[] storeContentBytes)
        {
            logger.MethodEntry(LogLevel.Debug);

            X509CertificateEntry certificateEntry;

            try
            {
                CertificateConverter converter = CertificateConverterFactory.FromDER(storeContentBytes);
                X509Certificate bcCert = converter.ToBouncyCastleCertificate();
                certificateEntry = new X509CertificateEntry(bcCert);
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to retrieve certificate.", ex);
            }

            logger.MethodExit(LogLevel.Debug);

            return certificateEntry;
        }

        private AsymmetricKeyEntry GetPrivateKey(string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            byte[] privateKeyContents = remoteHandler.DownloadCertificateFile(SeparatePrivateKeyFilePath);

            if (privateKeyContents.Length == 0)
                throw new RemoteFileException("Invalid private key: No private key found.");

            PrivateKeyConverter converter = PrivateKeyConverterFactory.FromPkcs8Blob(privateKeyContents, storePassword);

            logger.MethodExit(LogLevel.Debug);

            return new AsymmetricKeyEntry(converter.ToBCPrivateKey());
        }
    }
}
