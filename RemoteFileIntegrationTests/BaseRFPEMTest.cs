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
        public void CreateStore(string fileName, bool withExtKeyFile, bool withCertificate)
        {
            CreateFile("EmptyInternalPEM.pem", null);
        }
    }
}
