using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer(string storeProperties)
        {
            return new KDBCertificateStoreSerializer(storeProperties);
        }

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }
    }
}
