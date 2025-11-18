using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFileIntegrationTests
{
    public abstract class BaseRFPEMTest : BaseTest
    {
        private string pemCertificate = string.Empty;
        private string pemKey = string.Empty;

        public BaseRFPEMTest()
        {
            CreateCertificateAndKey();
        }

        public void CreateStore(string fileName, bool withExtKeyFile, bool withCertificate)
        {
            string storeContents = withCertificate ? (withExtKeyFile ? pemCertificate + System.Environment.NewLine + pemKey : pemCertificate) : string.Empty;
            CreateFile($"{fileName}.pem", Encoding.ASCII.GetBytes(storeContents));
            if (withExtKeyFile)
                CreateFile($"{fileName}.key", Encoding.ASCII.GetBytes(pemKey));
        }

        private void CreateCertificateAndKey()
        {
            pemCertificate = string.Empty;
            pemKey = string.Empty;
        }
    }
}
