using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class RemoteFileJobTypeBase
    {
        public IPAMSecretResolver _resolver;
        internal abstract ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties);
    }
}
