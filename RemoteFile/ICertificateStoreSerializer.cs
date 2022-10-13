using Org.BouncyCastle.Pkcs;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using System.Collections.Generic;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    interface ICertificateStoreSerializer
    {
        Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, string storeProperties, IRemoteHandler remoteHandler);

        List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storePassword, string storeProperties, IRemoteHandler remoteHandler);
    }
}
