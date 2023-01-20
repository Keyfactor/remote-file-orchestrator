using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new PKCS12CertificateStoreSerializer(storeProperties);
        }

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
