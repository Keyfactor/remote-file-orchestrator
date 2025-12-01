using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFileIntegrationTests
{
    internal class EnvironmentVariables
    {
        public static string? LinuxServer { get { return Environment.GetEnvironmentVariable("LinuxServer"); } }
        public static string? LinuxStorePath { get { return Environment.GetEnvironmentVariable("LinuxStorePath"); } }
        public static string? LinuxUserId { get { return Environment.GetEnvironmentVariable("LinuxUserId"); } }
        public static string? LinuxUserPassword { get { return Environment.GetEnvironmentVariable("LinuxUserPassword"); } }
        public static string? WindowsServer { get { return Environment.GetEnvironmentVariable("WindowsServer"); } }
        public static string? WindowsStorePath { get { return Environment.GetEnvironmentVariable("WindowsStorePath"); } }
        public static string? StorePassword { get { return Environment.GetEnvironmentVariable("StorePassword"); } }
        public static string? PrivateKeyPassword { get { return Environment.GetEnvironmentVariable("PrivateKeyPassword"); } }
        public static string? ExistingCertificateSubjectDN { get { return Environment.GetEnvironmentVariable("ExistingCertificateSubjectDN"); } }
        public static string? NewCertificaetSubjectDN { get { return Environment.GetEnvironmentVariable("NewCertificaetSubjectDN"); } }
    }
}
