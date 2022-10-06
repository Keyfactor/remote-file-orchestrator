using System.Security.Cryptography.X509Certificates;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.Models
{
    class SerializedStoreInfo : X509Certificate2
    {
        public string FilePath { get; set; }

        public byte[] Contents { get; set; }
    }
}
