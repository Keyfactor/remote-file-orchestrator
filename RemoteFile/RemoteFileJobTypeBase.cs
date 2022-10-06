using System;
using System.Collections.Generic;
using System.Text;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class RemoteFileJobTypeBase
    {
        internal abstract ICertificateStoreSerializer GetCertificateStoreSerializer();
    }
}
