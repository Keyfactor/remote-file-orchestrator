﻿// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

using Keyfactor.Logging;
using Keyfactor.PKI.PrivateKeys;
using Keyfactor.PKI.X509;
using Keyfactor.PKI.PEM;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    class PEMCertificateStoreSerializer : ICertificateStoreSerializer
    {
        string[] PrivateKeyDelimetersPkcs8 = new string[] { "-----BEGIN PRIVATE KEY-----", "-----BEGIN ENCRYPTED PRIVATE KEY-----" };
        string[] PrivateKeyDelimetersPkcs1 = new string[] { "-----BEGIN RSA PRIVATE KEY-----" };
        string CertDelimBeg = "-----BEGIN CERTIFICATE-----";
        string CertDelimEnd = "-----END CERTIFICATE-----";

        private bool IsTrustStore { get; set; }
        private bool IncludesChain { get; set; }
        private string SeparatePrivateKeyFilePath { get; set; }
        private bool IgnorePrivateKeyOnInventory { get; set; }

        private ILogger logger;

        public PEMCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            LoadCustomProperties(storeProperties);
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            logger.MethodEntry(LogLevel.Debug);
           
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            string storeContents = Encoding.ASCII.GetString(storeContentBytes);
            X509CertificateEntry[] certificates = GetCertificates(storeContents);

            if (IsTrustStore || (isInventory && IgnorePrivateKeyOnInventory))
            {
                foreach(X509CertificateEntry certificate in certificates)
                {
                    store.SetCertificateEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificate.Certificate).ToX509Certificate2().Thumbprint, certificate);
                }
            }
            else
            {
                bool isRSAPrivateKey = false;
                AsymmetricKeyEntry keyEntry = GetPrivateKey(storeContents, storePassword ?? string.Empty, remoteHandler, out isRSAPrivateKey);

                if (isRSAPrivateKey && !string.IsNullOrEmpty(storePassword))
                    throw new RemoteFileException($"Certificate store with an RSA Private Key cannot contain a store password.  Invalid store format not supported.");

                store.SetKeyEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificates[0].Certificate).ToX509Certificate2().Thumbprint, keyEntry, certificates);
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

            string pemString = string.Empty;
            string keyString = string.Empty;
            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            if (IsTrustStore)
            {
                foreach (string alias in certificateStore.Aliases)
                {
                    if (certificateStore.IsKeyEntry(alias))
                        throw new RemoteFileException("Cannot add a certificate with a private key to a PEM trust store.");

                    CertificateConverter certConverter = CertificateConverterFactory.FromBouncyCastleCertificate(certificateStore.GetCertificate(alias).Certificate);
                    pemString += certConverter.ToPEM(true);
                }
            }
            else
            {
                string storeContents = Encoding.ASCII.GetString(remoteHandler.DownloadCertificateFile(storePath + storeFileName));
                bool isRSAPrivateKey = false;
                try
                {
                    GetPrivateKey(storeContents, storePassword, remoteHandler, out isRSAPrivateKey);
                }
                catch (RemoteFileException) { }

                if (isRSAPrivateKey && !string.IsNullOrEmpty(storePassword))
                    throw new RemoteFileException($"Certificate store with an RSA Private Key cannot contain a store password.  Invalid store format not supported.");

                bool keyEntryProcessed = false;
                foreach (string alias in certificateStore.Aliases)
                {
                    if (keyEntryProcessed)
                        throw new RemoteFileException("Cannot add a new certificate to a PEM store that already contains a certificate/key entry.");
                    else
                        keyEntryProcessed = true;

                    if (!certificateStore.IsKeyEntry(alias))
                        throw new RemoteFileException("No private key found.  Private key must be present to add entry to a non-Trust PEM certificate store.");

                    X509CertificateEntry[] chainEntries = certificateStore.GetCertificateChain(alias);
                    CertificateConverter certConverter = CertificateConverterFactory.FromBouncyCastleCertificate(chainEntries[0].Certificate);

                    AsymmetricKeyParameter privateKey = certificateStore.GetKey(alias).Key;
                    X509CertificateEntry[] certEntries = certificateStore.GetCertificateChain(alias);
                    AsymmetricKeyParameter publicKey = certEntries[0].Certificate.GetPublicKey();

                    if (isRSAPrivateKey)
                    {
                        TextWriter textWriter = new StringWriter();
                        PemWriter pemWriter = new PemWriter(textWriter);
                        pemWriter.WriteObject(privateKey);
                        pemWriter.Writer.Flush();

                        keyString = textWriter.ToString();
                    }
                    else
                    {
                        PrivateKeyConverter keyConverter = PrivateKeyConverterFactory.FromBCKeyPair(privateKey, publicKey, false);

                        byte[] privateKeyBytes = string.IsNullOrEmpty(storePassword) ? keyConverter.ToPkcs8BlobUnencrypted() : keyConverter.ToPkcs8Blob(storePassword);
                        keyString = PemUtilities.DERToPEM(privateKeyBytes, string.IsNullOrEmpty(storePassword) ? PemUtilities.PemObjectType.PrivateKey : PemUtilities.PemObjectType.EncryptedPrivateKey);
                    }

                    pemString = certConverter.ToPEM(true);
                    if (string.IsNullOrEmpty(SeparatePrivateKeyFilePath))
                        pemString += keyString;

                    if (IncludesChain)
                    {
                        for (int i = 1; i < chainEntries.Length; i++)
                        {
                            CertificateConverter chainConverter = CertificateConverterFactory.FromBouncyCastleCertificate(chainEntries[i].Certificate);
                            pemString += chainConverter.ToPEM(true);
                        }
                    }
                }
            }

            storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath+storeFileName, Contents = Encoding.ASCII.GetBytes(pemString) });
            if (!string.IsNullOrEmpty(SeparatePrivateKeyFilePath))
                storeInfo.Add(new SerializedStoreInfo() { FilePath = SeparatePrivateKeyFilePath, Contents = Encoding.ASCII.GetBytes(keyString) });

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
            IsTrustStore = properties.IsTrustStore == null || string.IsNullOrEmpty(properties.IsTrustStore.Value) ? false : bool.Parse(properties.IsTrustStore.Value);
            IncludesChain = properties.IncludesChain == null || string.IsNullOrEmpty(properties.IncludesChain.Value) ? false : bool.Parse(properties.IncludesChain.Value);
            SeparatePrivateKeyFilePath = properties.SeparatePrivateKeyFilePath == null || string.IsNullOrEmpty(properties.SeparatePrivateKeyFilePath.Value) ? String.Empty : properties.SeparatePrivateKeyFilePath.Value;
            IgnorePrivateKeyOnInventory = properties.IgnorePrivateKeyOnInventory == null || string.IsNullOrEmpty(properties.IgnorePrivateKeyOnInventory.Value) ? false : bool.Parse(properties.IgnorePrivateKeyOnInventory.Value);

            logger.MethodExit(LogLevel.Debug);
        }

        private X509CertificateEntry[] GetCertificates(string certificates)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<X509CertificateEntry> certificateEntries = new List<X509CertificateEntry>();

            try
            {
                while (certificates.Contains(CertDelimBeg))
                {
                    int certStart = certificates.IndexOf(CertDelimBeg);
                    int certLength = certificates.IndexOf(CertDelimEnd) + CertDelimEnd.Length - certStart;
                    string certificate = certificates.Substring(certStart, certLength);

                    CertificateConverter c2 = CertificateConverterFactory.FromPEM(Encoding.ASCII.GetBytes(certificate.Replace(CertDelimBeg, string.Empty).Replace(CertDelimEnd, string.Empty)));
                    X509Certificate bcCert = c2.ToBouncyCastleCertificate();
                    certificateEntries.Add(new X509CertificateEntry(bcCert));

                    certificates = certificates.Substring(certStart + certLength - 1);
                }
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to retrieve certificate chain.", ex);
            }

            logger.MethodExit(LogLevel.Debug);

            return certificateEntries.ToArray();
        }

        private AsymmetricKeyEntry GetPrivateKey(string storeContents, string storePassword, IRemoteHandler remoteHandler, out bool isRSA)
        {
            logger.MethodEntry(LogLevel.Debug);

            if (!String.IsNullOrEmpty(SeparatePrivateKeyFilePath))
            {
                storeContents = Encoding.ASCII.GetString(remoteHandler.DownloadCertificateFile(SeparatePrivateKeyFilePath));
            }

            isRSA = false;
            foreach (string begDelim in PrivateKeyDelimetersPkcs1)
            {
                if (storeContents.Contains(begDelim))
                {
                    isRSA = true;
                    break;
                }
            }

            string privateKey = string.Empty;
            foreach (string begDelim in isRSA ? PrivateKeyDelimetersPkcs1 : PrivateKeyDelimetersPkcs8)
            {
                string endDelim = begDelim.Replace("BEGIN", "END");

                int keyStart = storeContents.IndexOf(begDelim);
                if (keyStart == -1)
                    continue;
                int keyLength = storeContents.IndexOf(endDelim) + endDelim.Length - keyStart;
                if (keyLength == -1)
                    throw new RemoteFileException("Invalid private key: No ending private key delimiter found.");

                privateKey = storeContents.Substring(keyStart, keyLength).Replace(begDelim, string.Empty).Replace(endDelim, string.Empty);

                break;
            }

            if (string.IsNullOrEmpty(privateKey))
                throw new RemoteFileException("Invalid private key: No private key or invalid private key format found.");

            PrivateKeyConverter c;
            if (isRSA)
            {
                RSA rsa = RSA.Create();
                int bytesRead;
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out bytesRead);
                c = PrivateKeyConverterFactory.FromNetPrivateKey(rsa, false);
            }
            else
            {
                c = PrivateKeyConverterFactory.FromPkcs8Blob(Convert.FromBase64String(privateKey), storePassword);
            }

            logger.MethodExit(LogLevel.Debug);

            return new AsymmetricKeyEntry(c.ToBCPrivateKey());
        }
    }
}
