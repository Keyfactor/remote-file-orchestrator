
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.PEM
{
    public class Management : ManagemenBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new PEMCertificateStoreSerializer();
        }
    }
}
