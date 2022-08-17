using System.IO;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;

using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class PKCS12CertificateStoreSerializer : ICertificateStoreSerializer
    {
        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                store.Load(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }

            return store;
        }

        public byte[] SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePassword, string storeProperties, IRemoteHandler remoteHandler)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                certificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                return outStream.ToArray();
            }
        }
    }
}
