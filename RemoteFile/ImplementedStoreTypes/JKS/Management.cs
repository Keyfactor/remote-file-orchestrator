
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.JKS
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new JKSCertificateStoreSerializer();
        }
    }
}
