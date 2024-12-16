// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.Pkcs;

using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using System.Linq;
using Keyfactor.PKI.Extensions;
using Org.BouncyCastle.Asn1.Nist;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class PKCS12CertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;
        private bool HasEmptyAliases { get; set; }

        public PKCS12CertificateStoreSerializer(string storeProperties)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store workingStore = storeBuilder.Build();
            Pkcs12Store returnStore = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                workingStore.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }

            if (workingStore.Aliases.Where(p => string.IsNullOrEmpty(p)).Count() > 0 && workingStore.Aliases.Where(p => !string.IsNullOrEmpty(p)).Count() > 0)
                throw new Exception("Certificate store contains entries with both empty and non-empty friendly names.  This configuration is not supported in this store type.");

            HasEmptyAliases = workingStore.Aliases.Where(p => string.IsNullOrEmpty(p)).Count() > 0;

            returnStore = ConvertAliases(workingStore, true);

            return returnStore;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            storeBuilder.SetCertAlgorithm(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc);
            storeBuilder.SetKeyAlgorithm(NistObjectIdentifiers.IdAes256Cbc, PkcsObjectIdentifiers.IdHmacWithSha256);
            storeBuilder.SetUseDerEncoding(true);

            Pkcs12Store workingStore = storeBuilder.Build();

            foreach (string alias in certificateStore.Aliases)
            {
                if (certificateStore.IsKeyEntry(alias))
                {
                    workingStore.SetKeyEntry(alias, certificateStore.GetKey(alias), certificateStore.GetCertificateChain(alias));
                }
                else
                {
                    workingStore.SetCertificateEntry(alias, certificateStore.GetCertificate(alias));
                }
            }

            Pkcs12Store outputCertificateStore = ConvertAliases(workingStore, false);

            using (MemoryStream outStream = new MemoryStream())
            {
                outputCertificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());

                List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();
                storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath+storeFileName, Contents = outStream.ToArray() });
                
                return storeInfo;
            }
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }

        private Pkcs12Store ConvertAliases(Pkcs12Store workingStore, bool useThumbprintAsAlias)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            storeBuilder.SetCertAlgorithm(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc);
            storeBuilder.SetKeyAlgorithm(NistObjectIdentifiers.IdAes256Cbc, PkcsObjectIdentifiers.IdHmacWithSha256);
            storeBuilder.SetUseDerEncoding(true);

            Pkcs12Store returnStore = storeBuilder.Build();

            if (HasEmptyAliases)
            {
                foreach (string alias in workingStore.Aliases)
                {
                    if (workingStore.IsKeyEntry(alias))
                    {
                        X509CertificateEntry cert = workingStore.GetCertificate(alias);
                        returnStore.SetKeyEntry(useThumbprintAsAlias ? cert.Certificate.Thumbprint() : string.Empty, workingStore.GetKey(alias), workingStore.GetCertificateChain(alias));
                    }
                    else
                    {
                        X509CertificateEntry cert = workingStore.GetCertificate(alias);
                        returnStore.SetCertificateEntry(cert.Certificate.Thumbprint(), cert);
                    }
                }
            }
            else
            {
                returnStore = workingStore;
            }

            return returnStore;
        }
    }
}
