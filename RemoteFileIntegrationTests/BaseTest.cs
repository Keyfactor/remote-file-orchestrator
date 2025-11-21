using Moq;
using System.Runtime.CompilerServices;
using Renci.SshNet;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Keyfactor.Orchestrators.Extensions;

namespace RemoteFileIntegrationTests
{
    public abstract class BaseTest : IDisposable
    {
        public enum STORE_ENVIRONMENT_ENUM
        {
            LINUX,
            WINDOWS
        }

        private static ConnectionInfo Connection { get; set; }


        [ModuleInitializer]
        public static void Init()
        {
            var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envFile))
            {
                foreach (var line in File.ReadAllLines(envFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }
            else
            {
                throw new Exception(".env not found.  Make sure .env exists and is being copied to the run folder on compile.");
            }
        }

        public BaseTest()
        {
            if (Connection != null)
                return;

            string userId = EnvironmentVariables.LinuxUserId!;
            KeyboardInteractiveAuthenticationMethod keyboardAuthentication = new KeyboardInteractiveAuthenticationMethod(userId);
            Connection = new ConnectionInfo(EnvironmentVariables.LinuxServer, userId, new PasswordAuthenticationMethod(userId, EnvironmentVariables.LinuxUserPassword), keyboardAuthentication);

            SetUp();
        }

        public void CreateFile(string fileName, byte[] contents, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.LINUX) 
                CreateFileLinux(fileName, contents);
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.WINDOWS)
                CreateFileWindows(fileName, contents);
        }

        public void RemoveFile(string fileName, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.LINUX)
                RemoveFileLinux(fileName);
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.WINDOWS)
                RemoveFileWindows(fileName);
        }

        public void Dispose()
        {
            TearDown();
        }

        public abstract void SetUp();

        public abstract void TearDown();

        internal Mock<IPAMSecretResolver> GetMockSecretResolver(JobConfiguration config)
        {
            Mock<IPAMSecretResolver> secretResolver = new Mock<IPAMSecretResolver>();
            secretResolver
                .Setup(p => p.Resolve(It.Is<string>(q => q == config.ServerUsername)))
                .Returns(config.ServerUsername);
            secretResolver
                .Setup(p => p.Resolve(It.Is<string>(q => q == config.ServerPassword)))
                .Returns(config.ServerPassword);

            return secretResolver;
        }

        private void CreateFileLinux(string fileName, byte[] contents)
        {
            using (ScpClient client = new ScpClient(Connection))
            {
                try
                {
                    client.OperationTimeout = System.TimeSpan.FromSeconds(60);
                    client.Connect();

                    using (MemoryStream stream = new MemoryStream(contents))
                    {
                        client.Upload(stream, EnvironmentVariables.LinuxStorePath + fileName);
                    }
                }
                finally
                {
                    client.Disconnect();
                }
            };
        }

        private void CreateFileWindows(string fileName, byte[] contents)
        {

        }

        private void RemoveFileLinux(string fileName)
        {
            using (SftpClient client = new SftpClient(Connection))
            {
                try
                {
                    client.OperationTimeout = System.TimeSpan.FromSeconds(60);
                    client.Connect();
                    client.DeleteFile(EnvironmentVariables.LinuxStorePath + fileName);
                }
                finally
                {
                    client.Disconnect();
                }
            };
        }

        private void RemoveFileWindows(string fileName)
        {

        }
    }
}
