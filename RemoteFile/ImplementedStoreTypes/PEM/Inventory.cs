
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PEMCertificateStoreSerializer();
        }
    }
}
