
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PEMCertificateStoreSerializer();
        }
    }
}
