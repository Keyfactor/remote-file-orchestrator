using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;

using Moq;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace RemoteFileIntegrationTests
{
    public class RFPEMInventoryTests : BaseRFPEMTest
    {
        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_EmptyStore_Linux_Test0001()
        {
            InventoryJobConfiguration config = BuildBaseInventoryConfig();
            config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;
            config.CertificateStoreDetails.StorePath = EnvironmentVariables.LinuxStorePath;
            config.CertificateStoreDetails.Properties = "{}";
            //config.CertificateStoreDetails.Properties = JsonConvert.SerializeObject(new Dictionary<string, string?>() { { "SeparatePrivateKeyFilePath", Environment.GetEnvironmentVariable("LinuxStorePath") + "Test0001.key" } });
            config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;

            Mock<IPAMSecretResolver> secretResolver = GetMockSecretResolver(config);

            Mock<SubmitInventoryUpdate> submitInventoryUpdate = new Mock<SubmitInventoryUpdate>();

            Inventory inventory = new Inventory(secretResolver.Object);
            JobResult result = inventory.ProcessJob(config, submitInventoryUpdate.Object);

            Assert.Equal(OrchestratorJobStatusJobResult.Success, result.Result);
            
            IInvocation invocation = submitInventoryUpdate.Invocations[0];
            List<CurrentInventoryItem> inventoryItems = (List<CurrentInventoryItem>)invocation.Arguments[0];
            Assert.Single(inventoryItems);

            using (StringReader rdr = new StringReader(inventoryItems[0].Certificates.First()))
            {
                PemReader pemReader = new PemReader(rdr);
                PemObject pemObject = pemReader.ReadPemObject();
                X509CertificateParser parser = new X509CertificateParser();
                X509Certificate certificate = parser.ReadCertificate(pemObject.Content);

                Assert.Equal(EnvironmentVariables.CertificateSubjectDN, certificate.SubjectDN.ToString());
            }

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
            config.ServerUsername = EnvironmentVariables.LinuxUserId;
            config.ServerPassword = EnvironmentVariables.LinuxUserPassword;

            return config;
        }
    }
}