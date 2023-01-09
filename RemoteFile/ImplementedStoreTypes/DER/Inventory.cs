using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.DER
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new DERCertificateStoreSerializer();
        }

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
