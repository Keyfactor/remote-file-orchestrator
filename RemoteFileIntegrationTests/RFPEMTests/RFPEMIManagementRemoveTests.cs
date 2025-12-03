using Keyfactor.Extensions.Orchestrator.RemoteFile.PEM;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Keyfactor.PKI.X509;

using Moq;

using Newtonsoft.Json;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Pkcs;
using Keyfactor.PKI.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Text;

namespace Keyfactor.Extensions.Orchestrator.RemoteFileIntegrationTests.RFPEMTests
{
    public class RFPEMManagementRemoveTests : BaseRFPEMTest, IClassFixture<RFPEMManagementRemoveTestsFixture>
    {
        public static ManagementRemoveTestConfig[] TestConfigs = {
            new ManagementRemoveTestConfig() { FileName = "Test0012", UseExistingAlias = true, HasSeparatePrivateKey = false, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementRemoveTestConfig() { FileName = "Test0013", UseExistingAlias = true, HasSeparatePrivateKey = true, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementRemoveTestConfig() { FileName = "Test0014", UseExistingAlias = false, HasSeparatePrivateKey = false, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementRemoveTestConfig() { FileName = "Test0015", UseExistingAlias = false, HasSeparatePrivateKey = true, WithCertificate = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementRemoveTestConfig() { FileName = "Test0016", UseExistingAlias = true, HasSeparatePrivateKey = false, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementRemoveTestConfig() { FileName = "Test0017", UseExistingAlias = true, HasSeparatePrivateKey = true, WithCertificate = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
        };

        private string NewDummyAlias = "abc";

        public static string ExistingAlias {  get; set; }

        [Fact]
        public void RFPEM_ManagementRemove_ExistingAlias_InternalKey_NonEmptyStore()
        {
            RunTest(TestConfigs[0], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementRemove_ExistingAlias_ExternalKey_NonEmptyStore()
        {
            RunTest(TestConfigs[1], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementRemove_NonExistingAlias_InternalKey_NonEmptyStore()
        {
            RunTest(TestConfigs[2], OrchestratorJobStatusJobResult.Failure, 
                $"Alias {NewDummyAlias} does not exist in certificate store");
        }

        [Fact]
        public void RFPEM_ManagementRemove_NonExistingAlias_ExternalKey_NonEmptyStore()
        {
            RunTest(TestConfigs[3], OrchestratorJobStatusJobResult.Failure,
                $"Alias {NewDummyAlias} does not exist in certificate store");
        }

        [Fact]
        public void RFPEM_ManagementRemove_ExistingAlias_InternalKey_EmptyStore()
        {
            RunTest(TestConfigs[4], OrchestratorJobStatusJobResult.Failure,
                $"Alias {ExistingAlias} does not exist in certificate store");
        }

        [Fact]
        public void RFPEM_ManagementRemove_ExistingAlias_ExternalKey_EmptyStore()
        {
            RunTest(TestConfigs[5], OrchestratorJobStatusJobResult.Failure,
                $"Alias {ExistingAlias} does not exist in certificate store");
        }

        private void RunTest(ManagementRemoveTestConfig testConfig, OrchestratorJobStatusJobResult expectedResult, string expectedMessage)
        {
            ManagementJobConfiguration config = new ManagementJobConfiguration();
            config.Capability = "Management";
            config.OperationType = CertStoreOperationType.Remove;
            config.JobId = new Guid();
            config.ServerUsername = EnvironmentVariables.LinuxUserId;
            config.ServerPassword = EnvironmentVariables.LinuxUserPassword;

            config.JobProperties = new Dictionary<string, object>();

            config.JobCertificate = new ManagementJobCertificate();
            config.JobCertificate.Alias = testConfig.UseExistingAlias ? ExistingAlias : NewDummyAlias;
            config.JobCertificate.PrivateKeyPassword = EnvironmentVariables.PrivateKeyPassword;
            (config.JobCertificate.Contents, _) = GetNewCert();

            config.CertificateStoreDetails = new CertificateStore();
            config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;
            config.CertificateStoreDetails.StorePath = EnvironmentVariables.LinuxStorePath + $"{testConfig.FileName}.pem";
            config.CertificateStoreDetails.StorePassword = string.Empty;
            config.CertificateStoreDetails.Properties = "{}";
            if (testConfig.HasSeparatePrivateKey)
                config.CertificateStoreDetails.Properties = JsonConvert.SerializeObject(new Dictionary<string, string?>() { { "SeparatePrivateKeyFilePath", Environment.GetEnvironmentVariable("LinuxStorePath") + $"{testConfig.FileName}.key" } });
            else
                config.CertificateStoreDetails.ClientMachine = EnvironmentVariables.LinuxServer;

            Mock<IPAMSecretResolver> secretResolver = GetMockSecretResolver(config);

            Management management = new Management(secretResolver.Object);
            JobResult result = management.ProcessJob(config);

            Assert.Equal(expectedResult, result.Result);
            if (!string.IsNullOrEmpty(expectedMessage))
                Assert.Contains(expectedMessage, result.FailureMessage);

            if (expectedResult == OrchestratorJobStatusJobResult.Success)
            {
                byte[] certificateBytes = ReadFile(testConfig.FileName + ".pem", testConfig.StoreEnvironment);
                byte[] keyBytes = testConfig.HasSeparatePrivateKey ? ReadFile(testConfig.FileName + ".key", testConfig.StoreEnvironment) : [];
                string certificatePEM = Encoding.ASCII.GetString(certificateBytes) + (keyBytes.Length > 0 ? Encoding.ASCII.GetString(keyBytes) : string.Empty);
                Assert.Equal(0, certificatePEM.Split(new string[] { "BEGIN CERTIFICATE" }, StringSplitOptions.None).Length - 1);
                Assert.Equal(0, certificatePEM.Split(new string[] { "BEGIN PRIVATE KEY" }, StringSplitOptions.None).Length - 1);
            }
        }
    }

    public class ManagementRemoveTestConfig
    {
        internal string FileName { get; set; }
        internal bool UseExistingAlias { get; set; }
        internal bool HasSeparatePrivateKey { get; set; }
        internal bool WithCertificate {  get; set; }
        internal BaseTest.STORE_ENVIRONMENT_ENUM StoreEnvironment { get; set; }
    }

    public class RFPEMManagementRemoveTestsFixture : IDisposable
    {
        public RFPEMManagementRemoveTestsFixture()
        {
            RFPEMManagementRemoveTests.ExistingAlias = SetUp(EnvironmentVariables.ExistingCertificateSubjectDN ?? string.Empty, EnvironmentVariables.NewCertificaetSubjectDN ?? string.Empty);
        }

        public void Dispose()
        {
            TearDown();
        }

        private string SetUp(string certName, string newCertName)
        {
            string existingAlias = BaseRFPEMTest.CreateCertificateAndKey(certName, BaseRFPEMTest.CERT_TYPE_ENUM.PEM);
            string newAlias = BaseRFPEMTest.CreateCertificateAndKey(newCertName, BaseRFPEMTest.CERT_TYPE_ENUM.PFX);

            foreach(ManagementRemoveTestConfig config in RFPEMManagementRemoveTests.TestConfigs)
            {
                BaseRFPEMTest.CreateStore(config.FileName, config.HasSeparatePrivateKey, config.WithCertificate, config.StoreEnvironment);
            }

            return existingAlias;
        }

        private void TearDown()
        {
            foreach (ManagementRemoveTestConfig config in RFPEMManagementRemoveTests.TestConfigs)
            {
                BaseRFPEMTest.RemoveStore(config.FileName, config.HasSeparatePrivateKey, config.StoreEnvironment);
            }
        }
    }




}