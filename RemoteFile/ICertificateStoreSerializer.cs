using Org.BouncyCastle.Pkcs;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    interface ICertificateStoreSerializer
    {
        Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePassword, string storeProperties, IRemoteHandler remoteHandler);

        byte[] SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePassword, string storeProperties, IRemoteHandler remoteHandler);
    }
}
