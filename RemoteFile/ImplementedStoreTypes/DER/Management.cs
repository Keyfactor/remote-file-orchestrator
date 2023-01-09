using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PEMCertificateStoreSerializer();
        }

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
