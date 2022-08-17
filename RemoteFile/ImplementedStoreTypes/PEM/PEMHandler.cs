// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.PKI.PEM;
using Keyfactor.PKI.PrivateKeys;

using Org.BouncyCastle.Pkcs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    class PEMHandler
    {
        string[] PrivateKeyDelimeters = new string[] { "-----BEGIN PRIVATE KEY-----", "-----BEGIN ENCRYPTED PRIVATE KEY-----", "-----BEGIN RSA PRIVATE KEY-----" };
        string CertDelimBeg = "-----BEGIN CERTIFICATE-----";
        string CertDelimEnd = "-----END CERTIFICATE-----";

        public bool HasBinaryContent { get { return false; } }

        public bool HasPrivateKey(byte[] certificateBytes, byte[] privateKeyBytes)
        {
            bool rtn = false;

            byte[] compareBytes = privateKeyBytes == null ? certificateBytes : privateKeyBytes;
            string compareContents = Encoding.UTF8.GetString(compareBytes, 0, compareBytes.Length);

            foreach (string delim in PrivateKeyDelimeters)
            {
                if (compareContents.IndexOf(delim, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    rtn = true;
                    break;
                }
            }

            return rtn;
        }

        internal X509Certificate2Collection RetrieveCertificates(byte[] binaryCertificates, string storePassword)
        {
            try
            {
                X509Certificate2Collection certificateCollection = new X509Certificate2Collection();
                string certificates = Encoding.UTF8.GetString(binaryCertificates);

                while (certificates.Contains(CertDelimBeg))
                {
                    int certStart = certificates.IndexOf(CertDelimBeg);
                    int certLength = certificates.IndexOf(CertDelimEnd) + CertDelimEnd.Length - certStart;
                    certificateCollection.Add(new X509Certificate2Ext(Encoding.UTF8.GetBytes(certificates.Substring(certStart, certLength))));

                    certificates = certificates.Substring(certStart + certLength - 1);
                }

                return certificateCollection;
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to retrieve certificate chain.", ex);
            }
        }

        internal 

        public List<SSHFileInfo> CreateCertificatePacket(string certToAdd, string alias, string pfxPassword, string storePassword, bool hasSeparatePrivateKey)
        {
            List<SSHFileInfo> fileInfo = new List<SSHFileInfo>();
            byte[] certBytes = Convert.FromBase64String(certToAdd);

            X509Certificate2 cert = new X509Certificate2(certBytes, pfxPassword);

            byte[] certWithoutKeyBytes = cert.Export(X509ContentType.Cert);
            string certificatePem = PemUtilities.DERToPEM(certWithoutKeyBytes, PemUtilities.PemObjectType.Certificate);

            if (!string.IsNullOrEmpty(pfxPassword))
            {
                PrivateKeyConverter converter = PrivateKeyConverterFactory.FromPKCS12(certBytes, pfxPassword);
                byte[] privateKeyBytes = string.IsNullOrEmpty(storePassword) ? converter.ToPkcs8BlobUnencrypted() : converter.ToPkcs8Blob(storePassword);
                string privateKeyPem = PemUtilities.DERToPEM(privateKeyBytes, string.IsNullOrEmpty(storePassword) ? PemUtilities.PemObjectType.PrivateKey : PemUtilities.PemObjectType.EncryptedPrivateKey);

                if (hasSeparatePrivateKey)
                {
                    fileInfo.Add(new SSHFileInfo()
                    {
                        FileType = SSHFileInfo.FileTypeEnum.Certificate,
                        FileContents = certificatePem,
                        Alias = alias
                    });

                    fileInfo.Add(new SSHFileInfo()
                    {
                        FileType = SSHFileInfo.FileTypeEnum.PrivateKey,
                        FileContents = privateKeyPem
                    });
                }
                else
                {
                    fileInfo.Add(new SSHFileInfo()
                    {
                        FileType = SSHFileInfo.FileTypeEnum.Certificate,
                        FileContents = certificatePem + "\n" + privateKeyPem,
                        Alias = alias
                    });
                }
            }
            else
            {
                fileInfo.Add(new SSHFileInfo()
                {
                    FileType = SSHFileInfo.FileTypeEnum.Certificate,
                    FileContents = certificatePem,
                    Alias = alias
                });
            }

            return fileInfo;
        }

        public void AddCertificateToStore(List<SSHFileInfo> files, string storePath, string privateKeyPath, IRemoteHandler ssh, PEMStore.ServerTypeEnum serverType, bool overwrite, bool hasPrivateKey, bool isSingleCertificateStore)
        {
            SSHFileInfo certInfo = files.FirstOrDefault(p => p.FileType == SSHFileInfo.FileTypeEnum.Certificate);
            X509Certificate2 x509Cert = new X509Certificate2(Encoding.ASCII.GetBytes(certInfo.FileContents));

            if (isSingleCertificateStore)
            {
                ReplaceStoreContents(storePath, ssh, certInfo.FileContents, overwrite);
            }
            else
            {
                AddRemoveCertificate(serverType, storePath, ssh, certInfo.Alias, x509Cert.Thumbprint, certInfo.FileContents, privateKeyPath, hasPrivateKey, overwrite, true);
            }


            if (!string.IsNullOrEmpty(privateKeyPath) && files.Exists(p => p.FileType == SSHFileInfo.FileTypeEnum.PrivateKey))
            {
                SSHFileInfo keyInfo = files.FirstOrDefault(p => p.FileType == SSHFileInfo.FileTypeEnum.PrivateKey);
                ssh.UploadCertificateFile(privateKeyPath, Encoding.ASCII.GetBytes(keyInfo.FileContents));
            }
        }

        public void RemoveCertificate(PEMStore.ServerTypeEnum serverType, string storePath, string privateKeyPath, IRemoteHandler ssh, string alias, bool hasPrivateKey)
        {
            AddRemoveCertificate(serverType, storePath, ssh, alias, string.Empty, string.Empty, privateKeyPath, hasPrivateKey, false, false);

            if (!string.IsNullOrEmpty(privateKeyPath))
            {
                ssh.UploadCertificateFile(privateKeyPath, new byte[] { });
            }
        }

        public bool IsValidStore(string path, PEMStore.ServerTypeEnum serverType, IRemoteHandler ssh)
        {
            string command = serverType == PEMStore.ServerTypeEnum.Linux ? $"grep -i -- '{CertDelimBeg}' {path}" : $"cmd /r findstr /i /l /c:\"{CertDelimBeg}\" \"{path}\"";
            string result = ssh.RunCommand(command, null, ApplicationSettings.UseSudo, null);
            return result.IndexOf(CertDelimBeg) > -1;
        }

        private void AddRemoveCertificate(PEMStore.ServerTypeEnum serverType, string storePath, IRemoteHandler ssh, string alias, string thumbprint, string replacementCert, string privateKeyPath, bool hasPrivateKey, bool overwrite, bool isAdd)
        {
            bool certFound = false;

            byte[] storebytes = ssh.DownloadCertificateFile(storePath, HasBinaryContent);
            string storeContents = Encoding.ASCII.GetString(storebytes);

            if (hasPrivateKey && string.IsNullOrEmpty(privateKeyPath))
            {
                storeContents = RemoveAllPrivateKeys(storeContents);
            }

            string storeContentsParsing = storeContents;

            while (storeContentsParsing.Contains(CertDelimBeg))
            {
                int certStart = storeContentsParsing.IndexOf(CertDelimBeg);
                int certLength = storeContentsParsing.IndexOf(CertDelimEnd) + CertDelimEnd.Length - certStart;
                string currCertFromStore = storeContentsParsing.Substring(certStart, certLength);
                X509Certificate2 x509CurrCertFromStore = new X509Certificate2(Encoding.UTF8.GetBytes(currCertFromStore));

                if (x509CurrCertFromStore.Thumbprint == alias || x509CurrCertFromStore.Thumbprint == thumbprint)
                {
                    if (!overwrite && isAdd)
                    {
                        throw new PEMException("Certificate with this alias/thumbprint already exists in store.  Please select 'Overwrite' if you wish to replace this certificate.");
                    }

                    storeContents = storeContents.Replace(currCertFromStore, replacementCert);
                    certFound = true;
                    break;
                }

                storeContentsParsing = storeContentsParsing.Substring(certStart + certLength - 1);
            }

            if (!certFound && !isAdd)
            {
                throw new PEMException("Certificate with this alias/thumbprint does not exist in store.");
            }

            if (storeContents.IndexOf(replacementCert) == -1 && isAdd)
            {
                storeContents += ("\n" + replacementCert);
            }

            ssh.UploadCertificateFile(storePath, Encoding.ASCII.GetBytes(storeContents));
        }

        private void ReplaceStoreContents(string storePath, IRemoteHandler ssh, string fileContents, bool overwrite)
        {
            byte[] storebytes = ssh.DownloadCertificateFile(storePath, HasBinaryContent);
            string storeContents = Encoding.ASCII.GetString(storebytes);
            if (!overwrite && storeContents.IndexOf(CertDelimBeg, StringComparison.OrdinalIgnoreCase) > -1)
            {
                throw new PEMException("Certificate store currently contains one or more certificates.  Please select 'overwrite' to replace the contents of the store.");
            }

            ssh.UploadCertificateFile(storePath, Encoding.ASCII.GetBytes(fileContents));
        }

        private string RemoveAllPrivateKeys(string storeContents)
        {
            foreach(string begDelim in PrivateKeyDelimeters)
            {
                string endDelim = begDelim.Replace("BEGIN", "END");

                while (storeContents.Contains(begDelim))
                {
                    int keyStart = storeContents.IndexOf(begDelim);
                    int keyLength = storeContents.IndexOf(endDelim) + endDelim.Length - keyStart;
                    string key = storeContents.Substring(keyStart, keyLength);
                    storeContents = storeContents.Replace(key, string.Empty);
                }
            }

            return storeContents;
        }
    }
}
