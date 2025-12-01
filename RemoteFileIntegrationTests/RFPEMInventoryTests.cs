using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;

using Moq;

using Newtonsoft.Json;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities.IO.Pem;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace RemoteFileIntegrationTests
{
    public class RFPEMInventoryTests : BaseRFPEMTest, IClassFixture<RFPEMInventoryTestsFixture>
    {
        public static InventoryTestConfig[] TestConfigs = {
            new InventoryTestConfig() { FileName = "Test0001", HasSeparatePrivateKey = false, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new InventoryTestConfig() { FileName = "Test0002", HasSeparatePrivateKey = false, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new InventoryTestConfig() { FileName = "Test0003", HasSeparatePrivateKey = true, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new InventoryTestConfig() { FileName = "Test0004", HasSeparatePrivateKey = true, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
        };

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

        private void RunTest(InventoryTestConfig testConfig)
        {
            InventoryJobConfiguration config = new InventoryJobConfiguration();
            config.JobId = new Guid();
            config.Capability = "Inventory";
            config.ServerUsername = EnvironmentVariables.LinuxUserId;
            config.ServerPassword = EnvironmentVariables.LinuxUserPassword;
            config.JobProperties = new Dictionary<string, object>();

            config.CertificateStoreDetails = new CertificateStore();
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
    }

    public class InventoryTestConfig
    {
        internal string FileName { get; set; }
        internal bool HasSeparatePrivateKey { get; set; }
        internal bool WithCertificate { get; set; }
        internal BaseTest.STORE_ENVIRONMENT_ENUM StoreEnvironment { get; set; }
    }

    public class RFPEMInventoryTestsFixture : IDisposable
    {
        public RFPEMInventoryTestsFixture()
        {
            SetUp(EnvironmentVariables.ExistingCertificateSubjectDN ?? string.Empty);
        }

        public void Dispose()
        {
            TearDown();
        }

        private void SetUp(string certName)
        {
            BaseRFPEMTest.CreateCertificateAndKey(certName, BaseRFPEMTest.CERT_TYPE_ENUM.PEM);
            foreach(InventoryTestConfig config in RFPEMInventoryTests.TestConfigs)
            {
                BaseRFPEMTest.CreateStore(config.FileName, config.HasSeparatePrivateKey, config.WithCertificate, config.StoreEnvironment);
            }
        }

        private void TearDown()
        {
            foreach (InventoryTestConfig config in RFPEMInventoryTests.TestConfigs)
            {
                BaseRFPEMTest.RemoveStore(config.FileName, config.HasSeparatePrivateKey, config.StoreEnvironment);
            }
        }
    }




}