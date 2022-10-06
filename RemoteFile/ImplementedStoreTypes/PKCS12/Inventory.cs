
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PKCS12CertificateStoreSerializer();
        }
    }
}
