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

        public void CreateStore(string fileName, bool withExtKeyFile, bool withCertificate, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            string storeContents = withCertificate ? (withExtKeyFile ? pemCertificate + System.Environment.NewLine + pemKey : pemCertificate) : string.Empty;
            CreateFile($"{fileName}.pem", Encoding.ASCII.GetBytes(storeContents), storeEnvironment);
            if (withExtKeyFile)
                CreateFile($"{fileName}.key", Encoding.ASCII.GetBytes(pemKey), storeEnvironment);
        }

        public void RemoveStore(string fileName, bool withExtKeyFile, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            RemoveFile($"{fileName}.pem", storeEnvironment);
            if (withExtKeyFile)
                RemoveFile($"{fileName}.key", storeEnvironment);
        }

        private void CreateCertificateAndKey()
        {
            pemCertificate = string.Empty;
            pemKey = string.Empty;
        }
    }
}
