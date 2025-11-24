using Keyfactor.PKI.Extensions;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

using System.Text;

namespace RemoteFileIntegrationTests
{
    public class BaseRFPEMTest : BaseTest
    {
        private static string pemCertificate = string.Empty;
        private static string pemKey = string.Empty;


        public static void CreateStore(string fileName, bool withExtKeyFile, bool withCertificate, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            string storeContents = withCertificate ? (withExtKeyFile ? pemCertificate : pemCertificate + System.Environment.NewLine + pemKey) : string.Empty;
            CreateFile($"{fileName}.pem", Encoding.ASCII.GetBytes(storeContents), storeEnvironment);
            if (withExtKeyFile)
                CreateFile($"{fileName}.key", Encoding.ASCII.GetBytes(withCertificate ? pemKey : string.Empty), storeEnvironment);
        }

        public static void RemoveStore(string fileName, bool withExtKeyFile, STORE_ENVIRONMENT_ENUM storeEnvironment)
        {
            RemoveFile($"{fileName}.pem", storeEnvironment);
            if (withExtKeyFile)
                RemoveFile($"{fileName}.key", storeEnvironment);
        }

        public static string CreateCertificateAndKey(string certNameString)
        {
            if (!string.IsNullOrEmpty(pemCertificate))
                return string.Empty;

            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

            // Define certificate attributes
            var certName = new X509Name(certNameString);
            BigInteger serialNumber = BigInteger.ProbablePrime(120, new Random());

            // Validity period
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(1);

            // Generate the certificate
            var certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(serialNumber);
            certGen.SetSubjectDN(certName);
            certGen.SetIssuerDN(certName); // Self-signed
            certGen.SetNotBefore(notBefore);
            certGen.SetNotAfter(notAfter);
            certGen.SetPublicKey(keyPair.Public);

            // Generate the certificate
            X509Certificate certificate = certGen.Generate(new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private));

            // Export certificate as PEM
            using (var sw = new StringWriter())
            {
                var pw = new PemWriter(sw);
                pw.WriteObject(certificate);
                pw.Writer.Flush();
                pemCertificate = sw.ToString();
            }

            // Export private key as PEM (unencrypted)
            using (var sw = new StringWriter())
            {
                var pw = new PemWriter(sw);
                pw.WriteObject((new Pkcs8Generator(keyPair.Private)).Generate());
                pw.Writer.Flush();
                pemKey = sw.ToString();
            }

            return certificate.Thumbprint();
        }
    }
}
