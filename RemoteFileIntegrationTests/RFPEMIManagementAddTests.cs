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

namespace RemoteFileIntegrationTests
{
    public class RFPEMManagementAddTests : BaseRFPEMTest, IClassFixture<RFPEMManagementAddTestsFixture>
    {
        public static ManagementAddTestConfig[] TestConfigs = {
            new ManagementAddTestConfig() { FileName = "Test0005", UseExistingAlias = false, HasSeparatePrivateKey = false, WithCertificate = false, Overwrite = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0006", UseExistingAlias = false, HasSeparatePrivateKey = true, WithCertificate = false, Overwrite = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0007", UseExistingAlias = true, HasSeparatePrivateKey = false, WithCertificate = true, Overwrite = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0008", UseExistingAlias = true, HasSeparatePrivateKey = false, WithCertificate = true, Overwrite = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0009", UseExistingAlias = true, HasSeparatePrivateKey = true, WithCertificate = true, Overwrite = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0010", UseExistingAlias = false, HasSeparatePrivateKey = false, WithCertificate = true, Overwrite = false, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
            new ManagementAddTestConfig() { FileName = "Test0011", UseExistingAlias = false, HasSeparatePrivateKey = true, WithCertificate = true, Overwrite = true, StoreEnvironment = STORE_ENVIRONMENT_ENUM.LINUX},
        };

        public static string ExistingAlias {  get; set; }

        [Fact]
        public void RFPEM_ManagementAdd_NewAlias_InternalKey_EmptyStore_NoOverwrite()
        {
            RunTest(TestConfigs[0], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementAdd_NewAlias_ExternalKey_EmptyStore_NoOverwrite()
        {
            RunTest(TestConfigs[1], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementAdd_ExistingAlias_InternalKey_NonEmptyStore_NoOverwrite()
        {
            RunTest(TestConfigs[2], OrchestratorJobStatusJobResult.Warning, "");
        }

        [Fact]
        public void RFPEM_ManagementAdd_ExistingAlias_InternalKey_NonEmptyStore_YesOverwrite()
        {
            RunTest(TestConfigs[3], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementAdd_ExistingAlias_ExternalKey_NonEmptyStore_YesOverwrite()
        {
            RunTest(TestConfigs[4], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementAdd_NewAlias_InternalKey_NonEmptyStore_NoOverwrite()
        {
            RunTest(TestConfigs[5], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        [Fact]
        public void RFPEM_ManagementAdd_NewAlias_ExternalKey_NonEmptyStore_YesOverwrite()
        {
            RunTest(TestConfigs[6], OrchestratorJobStatusJobResult.Success, string.Empty);
        }

        private void RunTest(ManagementAddTestConfig testConfig, OrchestratorJobStatusJobResult expectedResult, string expectedMessage)
        {
            ManagementJobConfiguration config = new ManagementJobConfiguration();
            config.Capability = "Management";
            config.OperationType = CertStoreOperationType.Add;
            config.JobId = new Guid();
            config.ServerUsername = EnvironmentVariables.LinuxUserId;
            config.ServerPassword = EnvironmentVariables.LinuxUserPassword;

            config.JobProperties = new Dictionary<string, object>();

            config.JobCertificate = new ManagementJobCertificate();
            config.JobCertificate.Alias = testConfig.UseExistingAlias ? ExistingAlias : string.Empty;
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
                Assert.Equal(expectedMessage, result.FailureMessage);

            if (expectedResult == OrchestratorJobStatusJobResult.Success)
            {
                byte[] certificateBytes = ReadFile(testConfig.FileName + ".pem", testConfig.StoreEnvironment);
                byte[] keyBytes = testConfig.HasSeparatePrivateKey ? ReadFile(testConfig.FileName + ".key", testConfig.StoreEnvironment) : [];
                string certificatePEM = Convert.ToBase64String(certificateBytes) + (keyBytes.Length > 0 ? Convert.ToBase64String(keyBytes) : string.Empty);
                Assert.Equal(1, certificatePEM.Split(new string[] { "BEGIN CERTIFICATE" }, StringSplitOptions.None).Length - 1);
                Assert.Equal(1, certificatePEM.Split(new string[] { "BEGIN PRIVATE KEY" }, StringSplitOptions.None).Length - 1);

                CertificateConverter converter = CertificateConverterFactory.FromPEM(certificatePEM);
                X509Certificate certificate = converter.ToBouncyCastleCertificate();
                (_, string thumbprint) = GetNewCert();
                Assert.Equal(thumbprint, certificate.Thumbprint());
            }
        }
    }

    public class ManagementAddTestConfig
    {
        internal string FileName { get; set; }
        internal bool UseExistingAlias { get; set; }
        internal bool HasSeparatePrivateKey { get; set; }
        internal bool WithCertificate { get; set; }
        internal bool Overwrite { get; set; }
        internal BaseTest.STORE_ENVIRONMENT_ENUM StoreEnvironment { get; set; }
    }

    public class RFPEMManagementAddTestsFixture : IDisposable
    {
        public RFPEMManagementAddTestsFixture()
        {
            RFPEMManagementAddTests.ExistingAlias = SetUp(EnvironmentVariables.ExistingCertificateSubjectDN ?? string.Empty, EnvironmentVariables.NewCertificaetSubjectDN ?? string.Empty);
        }

        public void Dispose()
        {
            TearDown();
        }

        private string SetUp(string certName, string newCertName)
        {
            string existingAlias = BaseRFPEMTest.CreateCertificateAndKey(certName, BaseRFPEMTest.CERT_TYPE_ENUM.PEM);
            string newAlias = BaseRFPEMTest.CreateCertificateAndKey(newCertName, BaseRFPEMTest.CERT_TYPE_ENUM.PFX);

            foreach(ManagementAddTestConfig config in RFPEMManagementAddTests.TestConfigs)
            {
                BaseRFPEMTest.CreateStore(config.FileName, config.HasSeparatePrivateKey, config.WithCertificate, config.StoreEnvironment);
            }

            return existingAlias;
        }

        private void TearDown()
        {
            foreach (ManagementAddTestConfig config in RFPEMManagementAddTests.TestConfigs)
            {
                BaseRFPEMTest.RemoveStore(config.FileName, config.HasSeparatePrivateKey, config.StoreEnvironment);
            }
        }
    }




}