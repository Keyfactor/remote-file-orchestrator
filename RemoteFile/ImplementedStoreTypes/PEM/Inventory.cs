using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new PEMCertificateStoreSerializer(storeProperties);
        }

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
