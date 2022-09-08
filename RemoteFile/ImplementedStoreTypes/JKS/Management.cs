
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.JKS
{
    public class Management : ManagemenBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new JKSCertificateStoreSerializer();
        }
    }
}
