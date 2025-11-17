using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFileIntegrationTests
{
    public abstract class BaseIntegrationTest : IDisposable
    {
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

        public BaseIntegrationTest()
        { 
        
        }

        public void Dispose()
        {
            TearDown();
        }

        public abstract void SetUp();

        public abstract void TearDown();
    }
}
