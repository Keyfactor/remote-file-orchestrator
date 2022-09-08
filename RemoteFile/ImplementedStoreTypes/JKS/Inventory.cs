
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.JKS
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new JKSCertificateStoreSerializer();
        }
    }
}
