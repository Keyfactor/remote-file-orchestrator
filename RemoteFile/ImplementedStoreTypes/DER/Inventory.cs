using Keyfactor.Orchestrators.Extensions.Interfaces;
using System.Management.Automation.Configuration;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.DER
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new DERCertificateStoreSerializer(storeProperties);
        }

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
