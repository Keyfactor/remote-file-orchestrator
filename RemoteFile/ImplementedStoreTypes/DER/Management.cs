using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.DER
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new DERCertificateStoreSerializer(storeProperties);
        }

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
