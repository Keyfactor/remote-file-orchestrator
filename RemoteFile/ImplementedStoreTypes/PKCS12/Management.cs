using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new PKCS12CertificateStoreSerializer(storeProperties);
        }

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
