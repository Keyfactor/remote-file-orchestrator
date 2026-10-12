// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.IO;
using System.Collections.Generic;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12;

using Keyfactor.Logging;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

using Microsoft.Extensions.Logging;
using Keyfactor.Orchestrators.Extensions;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.JKS
{
    class JKSCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;
        public JKSCertificateStoreSerializer(string storeProperties)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        private bool IsTypeJKS { get; set; }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            logger.MethodEntry(LogLevel.Debug);

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store pkcs12Store = storeBuilder.Build();
            Pkcs12Store pkcs12StoreNew = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                IsTypeJKS = new JksStore().Probe(ms);
            }

            if (IsTypeJKS)
            {
                logger.LogDebug("Store is of type JKS");
                JksStore jksStore = new JksStore();

                using (MemoryStream ms = new MemoryStream(storeContents))
                {
                    ms.Position = 0;
                    jksStore.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
                }

                foreach (string alias in jksStore.Aliases)
                {
                    if (jksStore.IsKeyEntry(alias))
                    {
                        AsymmetricKeyParameter keyParam = jksStore.GetKey(alias, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
                        AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(keyParam);

                        X509Certificate[] certificateChain = jksStore.GetCertificateChain(alias);
                        List<X509CertificateEntry> certificateChainEntries = new List<X509CertificateEntry>();
                        foreach (X509Certificate certificate in certificateChain)
                        {
                            certificateChainEntries.Add(new X509CertificateEntry(certificate));
                        }

                        pkcs12Store.SetKeyEntry(alias, keyEntry, certificateChainEntries.ToArray());
                    }
                    else
                    {
                        pkcs12Store.SetCertificateEntry(alias, new X509CertificateEntry(jksStore.GetCertificate(alias)));
                    }
                }

                // Second Pkcs12Store necessary because of an obscure BC bug where creating a Pkcs12Store without .Load (code above using "Set" methods only) does not set all internal hashtables necessary to avoid an error later
                //  when processing store.
                MemoryStream ms2 = new MemoryStream();
                pkcs12Store.Save(ms2, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                ms2.Position = 0;

                pkcs12StoreNew.Load(ms2, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }
            else
            {
                logger.LogDebug("Store is of type PKCS12");
                PKCS12CertificateStoreSerializer pkcs12Serializer = new PKCS12CertificateStoreSerializer(string.Empty);
                pkcs12StoreNew = pkcs12Serializer.DeserializeRemoteCertificateStore(storeContents, storePath, storePassword, remoteHandler, isInventory);
            }

            logger.MethodExit(LogLevel.Debug);
            return pkcs12StoreNew;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            if (IsTypeJKS)
            {
                JksStore jksStore = new JksStore();

                foreach (string alias in certificateStore.Aliases)
                {
                    if (certificateStore.IsKeyEntry(alias))
                    {
                        AsymmetricKeyEntry keyEntry = certificateStore.GetKey(alias);
                        X509CertificateEntry[] certificateChain = certificateStore.GetCertificateChain(alias);

                        List<X509Certificate> certificates = new List<X509Certificate>();
                        foreach (X509CertificateEntry certificateEntry in certificateChain)
                        {
                            certificates.Add(certificateEntry.Certificate);
                        }

                        jksStore.SetKeyEntry(alias, keyEntry.Key, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), certificates.ToArray());
                    }
                    else
                    {
                        jksStore.SetCertificateEntry(alias, certificateStore.GetCertificate(alias).Certificate);
                    }
                }

                using (MemoryStream outStream = new MemoryStream())
                {
                    jksStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());

                    storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath + storeFileName, Contents = outStream.ToArray() });

                    logger.MethodExit(LogLevel.Debug);
                    return storeInfo;
                }
            }
            else
            {
                PKCS12CertificateStoreSerializer pkcs12Serializer = new PKCS12CertificateStoreSerializer(string.Empty);
                storeInfo = pkcs12Serializer.SerializeRemoteCertificateStore(certificateStore, storePath, storeFileName, storePassword, remoteHandler);
            }

            return storeInfo;
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }
    }
}
