using System.IO;

using Keyfactor.Orchestrators.Extensions;

using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    class Inventory : RemoteFile.InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PKCS12CertificateStoreSerializer();
        }
    }
}
