
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    public class Management : ManagementBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new KDBCertificateStoreSerializer();
        }
    }
}
