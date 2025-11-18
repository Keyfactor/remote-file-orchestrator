
using System.Runtime.CompilerServices;
using Renci.SshNet;

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
        private static SshClient Client { get; set; }


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

            string userId = Environment.GetEnvironmentVariable("LinuxUserId")!;
            KeyboardInteractiveAuthenticationMethod keyboardAuthentication = new KeyboardInteractiveAuthenticationMethod(userId);
            Connection = new ConnectionInfo(Environment.GetEnvironmentVariable("LinuxServer"), userId, new PasswordAuthenticationMethod(userId, Environment.GetEnvironmentVariable("LinuxUserPassword")), keyboardAuthentication);
            Client = new SshClient(Connection);

            SetUp();
        }

        public void CreateFile(string fileName, byte[] contents, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.LINUX) 
                CreateFileLinux(fileName, contents);
            if (storeEnvironment == STORE_ENVIRONMENT_ENUM.WINDOWS)
                CreateFileWindows(fileName, contents);
        }

        public void Dispose()
        {
            TearDown();
        }

        public abstract void SetUp();

        public abstract void TearDown();

        private void CreateFileLinux(string fileName, byte[] contents)
        {

        }

        private void CreateFileWindows(string fileName, byte[] contents)
        {

        }
    }
}
