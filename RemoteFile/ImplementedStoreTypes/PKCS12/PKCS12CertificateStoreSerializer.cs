using System.IO;
using System.Collections.Generic;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class PKCS12CertificateStoreSerializer : ICertificateStoreSerializer
    {
        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                store.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }

            return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                certificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());

                List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();
                storeInfo.Add(new SerializedStoreInfo() { FilePath = storePath, Contents = outStream.ToArray() });
                
                return storeInfo;
            }
        }
    }
}
