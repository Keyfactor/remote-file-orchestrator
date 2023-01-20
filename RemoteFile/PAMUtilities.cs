using Keyfactor.Orchestrators.Extensions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    internal class PAMUtilities
    {
        internal static string ResolvePAMField(IPAMSecretResolver resolver, ILogger logger, string name, string key)
        {
            logger.LogDebug($"Attempting to resolve PAM eligible field {name}");
            return string.IsNullOrEmpty(key) ? key : resolver.Resolve(key);
        }
    }
}
