using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;

namespace RemoteFileTypeHandlers.RFKDB
{
    public class RFKDBCertificateStoreTypeFactory : ICertificateStoreTypeFactory
    {
        const String CONFIG_CONSTANT = """
        {
          "UseSudo": "N",
          "DefaultSudoImpersonatedUser": "",
          "CreateStoreIfMissing": "N",
          "UseNegotiate": "N",
          "SeparateUploadFilePath": "",
          "FileTransferProtocol": "BOTH",
          "DefaultLinuxPermissionsOnStoreCreation": "600",
          "DefaultOwnerOnStoreCreation": "",
          "SSHPort": "",
          "PostJobCommands": [
            { "Name": "Apache Tomcat Restart", "Environment": "Linux", "Command": "sudo systemctl restart tomcat" },
            { "Name": "Apache HTTPD Restart", "Environment": "Linux", "Command": "sudo systemctl restart httpd" },
            { "Name": "NGNIX Restart", "Environment": "Linux", "Command": "sudo systemctl restart nginx" },
            { "Name": "HAProxy Restart", "Environment": "Linux", "Command": "sudo systemctl restart haproxy" },
            { "Name": "Envoy Proxy Restart", "Environment": "Linux", "Command": "sudo systemctl restart envoy" },
            { "Name": "Jetty Restart", "Environment": "Linux", "Command": "sudo systemctl restart jetty" }
          ]
        }
        """;

        public static List<CertificateStoreTypeProperty> CustomFields { get; } = new List<CertificateStoreTypeProperty>()
            {
                new CertificateStoreTypeProperty() { Name = "ServerUsername", DisplayName = "Server Username", Type = CertificateStorePropertyDataType.Secret, DependsOn = "", DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "ServerPassword", DisplayName = "Server Password", Type = CertificateStorePropertyDataType.Secret, DependsOn = "", DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "CommandPath", DisplayName = "GSKCAPICMD installation folder path", DependsOn = "", Type = CertificateStorePropertyDataType.String, DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "LinuxFilePermissionsOnStoreCreation", DisplayName = "Linux File Permissions on Store Creation", DependsOn = "", Type = CertificateStorePropertyDataType.String, DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "LinuxFileOwnerOnStoreCreation", DisplayName = "Linux File Owner on Store Creation", DependsOn = "", Type = CertificateStorePropertyDataType.String, DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "SudoImpersonatedUser", DisplayName = "Sudo Impersonating User", DependsOn = "", Type = CertificateStorePropertyDataType.String, DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "RemoveRootCertificate", DisplayName = "Remove Root Certificate from Chain", DependsOn = "", Type = CertificateStorePropertyDataType.Bool, DefaultValue = "False" },
                new CertificateStoreTypeProperty() { Name = "IncludePortInSPN", DisplayName = "Include Port in SPN for WinRM", DependsOn = "", Type = CertificateStorePropertyDataType.Bool, DefaultValue = "False" },
                new CertificateStoreTypeProperty() { Name = "FileTransferProtocol", DisplayName = "File Transfer Protocol to Use", DependsOn = "", Type = CertificateStorePropertyDataType.MultipleChoice, DefaultValue = "SCP,SFTP,Both" },
                new CertificateStoreTypeProperty() { Name = "SSHPort", DisplayName = "SSH Port", DependsOn = "", Type = CertificateStorePropertyDataType.String, DefaultValue = "" },
                new CertificateStoreTypeProperty() { Name = "UseShellCommands", DisplayName = "Use Shell Commands", DependsOn = "", Type = CertificateStorePropertyDataType.Bool, DefaultValue = "True" },
                new CertificateStoreTypeProperty() { Name = "PostJobApplicationRestart", DisplayName = "Post Job Application Restart", DependsOn = "", Type = CertificateStorePropertyDataType.MultipleChoice, DefaultValue = "None" }
            };

        public CertificateStoreTypeSeed Create()
        {
            CertificateStoreTypeSeed storeType = new CertificateStoreTypeSeed()
            {
                CertificateFormat = CertificateFormat.PFX,
                Name = "RFKDB",
                ShortName = "RFKDB",
                Capability = "RFKDB",
                CustomAliasHandling = 2,
                PrivateKeyHandling = CertificateStorePrivateKeyOptions.Optional,
                StorePathType = StorePathTypes.Freeform,
                SupportedOperations = new SupportedOperations()
                {
                    Add = true,
                    Create = true,
                    Discovery = true,
                    Enrollment = true,
                    Remove = true
                },
                PasswordOptions = new StorePasswordOptions()
                {
                    Style = CertificateStoreTypesPasswordStyles.Default,
                    EntrySupported = false,
                    StoreRequired = true
                },
                GlobalConfiguration = CONFIG_CONSTANT
            };

            storeType.Properties = CustomFields;
            return storeType;
        }

        public void Dispose()
        {
        }
    }
}
