using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;

using Moq;

using Newtonsoft.Json;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities.IO.Pem;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.PowerShell.Commands;

namespace RemoteFileIntegrationTests
{
    public class RFPEMManagementAddTests : BaseRFPEMTest, IClassFixture<RFPEMManagementAddTestsFixture>
    {
        public static TestConfig[] TestConfigs = {
            new TestConfig() { FileName = "Test0001", HasSeparatePrivateKey = false, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new TestConfig() { FileName = "Test0002", HasSeparatePrivateKey = false, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new TestConfig() { FileName = "Test0003", HasSeparatePrivateKey = true, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new TestConfig() { FileName = "Test0004", HasSeparatePrivateKey = true, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
        };

        public static string ExistingAlias {  get; set; }

        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_EmptyStore_Linux_Test0001()
        {
            RunTest(TestConfigs[0]);
        }

        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_WithCert_Linux_Test0002()
        {
            RunTest(TestConfigs[1]);
        }

        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_EmptyStore_Linux_Test0003()
        {
            RunTest(TestConfigs[2]);
        }

        [Fact]
        public void RFPEM_Inventory_InternalPrivateKey_WithCert_Linux_Test0004()
        {
            RunTest(TestConfigs[3]);
        }

        private void RunTest(TestConfig testConfig)
        {
            InventoryJobConfiguration config = BuildBaseInventoryConfig(testConfig.WithCertificate ? ExistingAlias : string.Empty);
            config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;
            config.CertificateStoreDetails.StorePath = EnvironmentVariables.LinuxStorePath + $"{testConfig.FileName}.pem";
            config.CertificateStoreDetails.Properties = "{}";
            if (testConfig.HasSeparatePrivateKey)
                config.CertificateStoreDetails.Properties = JsonConvert.SerializeObject(new Dictionary<string, string?>() { { "SeparatePrivateKeyFilePath", Environment.GetEnvironmentVariable("LinuxStorePath") + $"{testConfig.FileName}.key" } });
            else
                config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;

            Mock<IPAMSecretResolver> secretResolver = GetMockSecretResolver(config);

            Mock<SubmitInventoryUpdate> submitInventoryUpdate = new Mock<SubmitInventoryUpdate>();

            Inventory inventory = new Inventory(secretResolver.Object);
            JobResult result = inventory.ProcessJob(config, submitInventoryUpdate.Object);

            Assert.Equal(OrchestratorJobStatusJobResult.Success, result.Result);

            if (testConfig.WithCertificate)
            {
                IInvocation invocation = submitInventoryUpdate.Invocations[0];
                List<CurrentInventoryItem> inventoryItems = (List<CurrentInventoryItem>)invocation.Arguments[0];
                Assert.Single(inventoryItems);

                using (StringReader rdr = new StringReader(inventoryItems[0].Certificates.First()))
                {
                    PemReader pemReader = new PemReader(rdr);
                    PemObject pemObject = pemReader.ReadPemObject();
                    X509CertificateParser parser = new X509CertificateParser();
                    X509Certificate certificate = parser.ReadCertificate(pemObject.Content);

                    Assert.Equal(EnvironmentVariables.ExistingCertificateSubjectDN, certificate.SubjectDN.ToString());
                }
            }
        }

        private ManagementJobConfiguration BuildBaseInventoryConfig()
        {
            ManagementJobConfiguration config = new ManagementJobConfiguration();
            config.JobCertificate = new ManagementJobCertificate();
            config.JobCertificate.Contents = ;
            config.Capability = "Management";
            config.CertificateStoreDetails = new CertificateStore();
            config.JobId = new Guid();
            config.JobProperties = new Dictionary<string, object>();
            config.ServerUsername = EnvironmentVariables.LinuxUserId;
            config.ServerPassword = EnvironmentVariables.LinuxUserPassword;

            return config;
        }

        public class TestConfig
        {
            internal string FileName { get; set; }
            internal bool HasSeparatePrivateKey { get; set; }
            internal bool WithCertificate { get; set; }
            internal bool Overwrite {  get; set; }
            internal BaseTest.STORE_ENVIRONMENT_ENUM StoreEnvironment { get; set; }
        }
    }

    public class RFPEMManagementAddTestsFixture : IDisposable
    {
        public RFPEMManagementAddTestsFixture()
        {
            RFPEMManagementAddTests.ExistingAlias = SetUp(EnvironmentVariables.ExistingCertificateSubjectDN ?? string.Empty);
        }

        public void Dispose()
        {
            TearDown();
        }

        private string SetUp(string certName)
        {
            string existingAlias = BaseRFPEMTest.CreateCertificateAndKey(certName);

            BaseRFPEMTest.CreateStore(RFPEMManagementAddTests.TestConfigs[0].FileName, RFPEMManagementAddTests.TestConfigs[0].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[0].WithCertificate, RFPEMManagementAddTests.TestConfigs[0].StoreEnvironment);
            BaseRFPEMTest.CreateStore(RFPEMManagementAddTests.TestConfigs[1].FileName, RFPEMManagementAddTests.TestConfigs[1].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[1].WithCertificate, RFPEMManagementAddTests.TestConfigs[1].StoreEnvironment);
            BaseRFPEMTest.CreateStore(RFPEMManagementAddTests.TestConfigs[2].FileName, RFPEMManagementAddTests.TestConfigs[2].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[2].WithCertificate, RFPEMManagementAddTests.TestConfigs[2].StoreEnvironment);
            BaseRFPEMTest.CreateStore(RFPEMManagementAddTests.TestConfigs[3].FileName, RFPEMManagementAddTests.TestConfigs[3].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[3].WithCertificate, RFPEMManagementAddTests.TestConfigs[3].StoreEnvironment);

            return existingAlias;
        }

        private void TearDown()
        {
            BaseRFPEMTest.RemoveStore(RFPEMManagementAddTests.TestConfigs[0].FileName, RFPEMManagementAddTests.TestConfigs[0].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[0].StoreEnvironment);
            BaseRFPEMTest.RemoveStore(RFPEMManagementAddTests.TestConfigs[1].FileName, RFPEMManagementAddTests.TestConfigs[1].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[1].StoreEnvironment);
            BaseRFPEMTest.RemoveStore(RFPEMManagementAddTests.TestConfigs[2].FileName, RFPEMManagementAddTests.TestConfigs[2].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[2].StoreEnvironment);
            BaseRFPEMTest.RemoveStore(RFPEMManagementAddTests.TestConfigs[3].FileName, RFPEMManagementAddTests.TestConfigs[3].HasSeparatePrivateKey, RFPEMManagementAddTests.TestConfigs[3].StoreEnvironment);
        }
    }




}