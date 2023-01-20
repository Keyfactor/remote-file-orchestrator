using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new PEMCertificateStoreSerializer(storeProperties);
        }

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
