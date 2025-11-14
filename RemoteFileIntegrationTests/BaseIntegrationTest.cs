using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFileIntegrationTests
{
    public abstract class BaseIntegrationTest : IDisposable
    {
        protected BaseIntegrationTest()
        { 
        
        }

        public void Dispose()
        {
            TearDown();
        }

        public abstract void TearDown();
    }
}
