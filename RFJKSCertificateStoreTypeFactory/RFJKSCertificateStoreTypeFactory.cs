using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;

namespace RemoteFileTypeHandlers.RFJKS
{
    public class RFJKSCertificateStoreTypeFactory : ICertificateStoreTypeFactory
    {
        /// <summary>
        /// This replaces the integration-manifest.  Returns an object representation of the store type.
        /// </summary>
        /// 

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
            {
              "Name": "Apache Tomcat Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart tomcat"
            },
            {
              "Name": "Apache HTTPD Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart httpd"
            },
            {
              "Name": "NGNIX Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart nginx"
            },
            {
              "Name": "HAProxy Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart haproxy"
            },
            {
              "Name": "Envoy Proxy Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart envoy"
            },
            {
              "Name": "Jetty Restart",
              "Environment": "Linux",
              "Command": "sudo systemctl restart jetty"
            }
          ]
        }
        """;

        public CertificateStoreTypeSeed Create()
        {
            CertificateStoreTypeSeed storeType = new CertificateStoreTypeSeed()
            {
                CertificateFormat = CertificateFormat.PFX,
                Name = "RFJKS",
                ShortName = "RFJKS",
                Capability = "RFJKS",
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

            List<CertificateStoreTypeProperty> properties = new List<CertificateStoreTypeProperty>()
            {
                new CertificateStoreTypeProperty()
                {
                    Name = "ServerUsername",
                    DisplayName = "Server Username",
                    Type = CertificateStorePropertyDataType.Secret,
                    DependsOn = "",
                    DefaultValue = "",
                    Required = false
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "ServerPassword",
                    DisplayName = "Server Password",
                    Type = CertificateStorePropertyDataType.Secret,
                    DependsOn = "",
                    DefaultValue = "",
                    Required = false
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "LinuxFilePermissionsOnStoreCreation",
                    DisplayName = "Linux File Permissions on Store Creation",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.String,
                    DefaultValue = ""
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "LinuxFileOwnerOnStoreCreation",
                    DisplayName = "Linux File Owner on Store Creation",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.String,
                    DefaultValue = ""
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "SudoImpersonatedUser",
                    DisplayName = "Sudo Impersonating User",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.String,
                    DefaultValue = ""
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "RemoveRootCertificate",
                    DisplayName = "Remove Root Certificate from Chain",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.Bool,
                    DefaultValue = "False"
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "IncludePortInSPN",
                    DisplayName = "Include Port in SPN for WinRM",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.Bool,
                    DefaultValue = "False"
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "FileTransferProtocol",
                    DisplayName = "File Transfer Protocol to Use",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.MultipleChoice,
                    DefaultValue = "SCP,SFTP,Both"
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "SSHPort",
                    DisplayName = "SSH Port",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.String,
                    DefaultValue = ""
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "UseShellCommands",
                    DisplayName = "Use Shell Commands",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.Bool,
                    DefaultValue = "True"
                },
                new CertificateStoreTypeProperty()
                {
                    Name = "PostJobApplicationRestart",
                    DisplayName = "Post Job Application Restart",
                    Required = false,
                    DependsOn = "",
                    Type = CertificateStorePropertyDataType.MultipleChoice,
                    DefaultValue = "Apache Tomcat Restart,Jetty Restart"
                }
            };

            storeType.Properties = properties;

            return storeType;
        }
        /// <summary>
        /// probably not needed to be implemented, but need to remove the NotImplementedException, just leave empty.  This is a place holder in case we need to dispose something.
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}
