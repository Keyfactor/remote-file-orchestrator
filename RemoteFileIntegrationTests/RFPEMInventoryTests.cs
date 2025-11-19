using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Moq;

namespace RemoteFileIntegrationTests
{
    public class RFPEMInventoryTests : BaseRFPEMTest
    {
        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_EmptyStore_Linux_Test0001()
        {
            InventoryJobConfiguration config = BuildBaseInventoryConfig();
            config.CertificateStoreDetails.ClientMachine = Environment.GetEnvironmentVariable("LinuxServer");
            config.CertificateStoreDetails.StorePath = Environment.GetEnvironmentVariable("LinuxStorePath");
            config.CertificateStoreDetails.Properties = "{}";
            //config.CertificateStoreDetails.Properties = JsonConvert.SerializeObject(new Dictionary<string, string?>() { { "SeparatePrivateKeyFilePath", Environment.GetEnvironmentVariable("LinuxStorePath") + "Test0001.key" } });
            config.CertificateStoreDetails.ClientMachine = Environment.GetEnvironmentVariable("LinuxServer");

            Mock<IPAMSecretResolver> secretResolver = GetMockSecretResolver(config);

            Mock<SubmitInventoryUpdate> submitInventoryUpdate = new Mock<SubmitInventoryUpdate>();

            Inventory inventory = new Inventory(secretResolver.Object);
            inventory.ProcessJob()
        }

        public override void SetUp()
        {
            CreateStore("Test0001", false, false, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0002", false, true, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0003", true, false, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0004", true, true, STORE_ENVIRONMENT_ENUM.LINUX);
        }

        public override void TearDown()
        {
            RemoveStore("Test0001", false, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0002", false, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0003", true, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0004", true, STORE_ENVIRONMENT_ENUM.LINUX);
        }

        private InventoryJobConfiguration BuildBaseInventoryConfig()
        {
            InventoryJobConfiguration config = new InventoryJobConfiguration();
            config.Capability = "Inventory";
            config.CertificateStoreDetails = new CertificateStore();
            config.JobId = new Guid();
            config.JobProperties = new Dictionary<string, object>();
            config.ServerUsername = Environment.GetEnvironmentVariable("LinuxUserId");
            config.ServerPassword = Environment.GetEnvironmentVariable("LinuxUserPassword");

            return config;
        }
    }
}