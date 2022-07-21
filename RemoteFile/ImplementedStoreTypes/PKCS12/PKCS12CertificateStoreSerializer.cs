using System;
using System.IO;

using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class PKCS12CertificateStoreSerializer : ICertificateStoreSerializer
    {
        Pkcs12Store ICertificateStoreSerializer.DeserializeRemoteCertificateStore(byte[] storeContents, string storePassword)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            using (MemoryStream ms = new MemoryStream(storeContents))
            {
                store.Load(ms, storePassword.ToCharArray());
            }

            return store;
        }

        byte[] ICertificateStoreSerializer.SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePassword)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                certificateStore.Save(outStream, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                return outStream.ToArray();
            }
        }
    }
}
