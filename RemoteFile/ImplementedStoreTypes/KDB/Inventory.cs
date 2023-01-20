using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new KDBCertificateStoreSerializer(storeProperties);
        }

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
