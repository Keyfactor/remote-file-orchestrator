
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PKCS12CertificateStoreSerializer();
        }
    }
}
