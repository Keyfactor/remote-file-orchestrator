using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    interface ICertificateStoreSerializer
    {
        internal Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePassword);

        internal byte[] SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePassword);
    }
}
