using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;

using Keyfactor.Logging;
using Keyfactor.PKI.PrivateKeys;
using Keyfactor.PKI.X509;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    class PEMCertificateStoreSerializer : ICertificateStoreSerializer
    {
        string[] PrivateKeyDelimeters = new string[] { "-----BEGIN PRIVATE KEY-----", "-----BEGIN ENCRYPTED PRIVATE KEY-----", "-----BEGIN RSA PRIVATE KEY-----" };
        string CertDelimBeg = "-----BEGIN CERTIFICATE-----";
        string CertDelimEnd = "-----END CERTIFICATE-----";

        private bool IsTrustStore { get; set; }
        private bool IncludesChain { get; set; }
        private string SeparatePrivateKeyFilePath { get; set; }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.MethodEntry(LogLevel.Debug);

            LoadCustomProperties(storeProperties);

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            string storeContents = Encoding.ASCII.GetString(storeContentBytes);
            X509CertificateEntry[] certificates = GetCertificates(storeContents);

            if (IsTrustStore)
            {
                foreach(X509CertificateEntry certificate in certificates)
                {
                    store.SetCertificateEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificate.Certificate).ToX509Certificate2().Thumbprint, certificate);
                }
            }
            else
            {
                AsymmetricKeyEntry keyEntry = GetPrivateKey(storeContents, storePassword, remoteHandler);
                store.SetKeyEntry(CertificateConverterFactory.FromBouncyCastleCertificate(certificates[0].Certificate).ToX509Certificate2().Thumbprint, keyEntry, certificates);
            }

            logger.MethodExit(LogLevel.Debug);
            return store;
        }

        public byte[] SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.MethodEntry(LogLevel.Debug);

            LoadCustomProperties(storeProperties);

            using (MemoryStream outStream = new MemoryStream())
            {
                certificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());

                logger.MethodExit(LogLevel.Debug);
                return outStream.ToArray();
            }
        }

        private void LoadCustomProperties(string storeProperties)
        {
            dynamic properties = JsonConvert.DeserializeObject(storeProperties);
            IsTrustStore = properties.IsTrustStore == null || string.IsNullOrEmpty(properties.IsTrustStore.Value) ? false : bool.Parse(properties.IsTrustStore.Value);
            IncludesChain = properties.IncludesChain == null || string.IsNullOrEmpty(properties.IncludesChain.Value) ? false : bool.Parse(properties.IncludesChain.Value);
        }

        private X509CertificateEntry[] GetCertificates(string certificates)
        {
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
                }
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to retrieve certificate chain.", ex);
            }

            return certificateEntries.ToArray();
        }

        private AsymmetricKeyEntry GetPrivateKey(string storeContents, string storePassword, IRemoteHandler remoteHandler)
        {
            if (String.IsNullOrEmpty(SeparatePrivateKeyFilePath))
            {
                storeContents = Encoding.ASCII.GetString(remoteHandler.DownloadCertificateFile(SeparatePrivateKeyFilePath));
            }

            string privateKey = string.Empty;
            foreach (string begDelim in PrivateKeyDelimeters)
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
                throw new RemoteFileException("Invalid private key: No private key found.");

            PrivateKeyConverter c = PrivateKeyConverterFactory.FromPkcs8Blob(Convert.FromBase64String(privateKey), storePassword);
            return new AsymmetricKeyEntry(c.ToBCPrivateKey());
        }
    }
}
