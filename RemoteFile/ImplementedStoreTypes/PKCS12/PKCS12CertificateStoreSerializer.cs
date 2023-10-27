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

using Org.BouncyCastle.Pkcs;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class PKCS12CertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;

        public PKCS12CertificateStoreSerializer(string storeProperties)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                store.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }

            return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                certificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());

                List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();
                storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath+storeFileName, Contents = outStream.ToArray() });
                
                return storeInfo;
            }
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }
    }
}
