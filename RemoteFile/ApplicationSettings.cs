using Microsoft.Extensions.Configuration;
using System;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    class ApplicationSettings
    {
        public enum FileTransferProtocolEnum
        {
            SCP,
            SFTP,
            Both
        }

        private const string DEFAULT_LINUX_PERMISSION_SETTING = "600";

        private static IConfiguration configuration;

        public static bool UseSudo { get { return configuration["UseSudo"]?.ToUpper() == "Y";  } }
        public static bool CreateStoreIfMissing { get { return configuration["CreateStoreIfMissing"]?.ToUpper() == "Y"; } }
        public static bool UseNegotiate { get { return configuration["UseNegotiate"]?.ToUpper() == "Y"; } }
        public static string SeparateUploadFilePath { get { return configuration["SeparateUploadFilePath"]; } }
        public static string DefaultLinuxPermissionsOnStoreCreation { get { return configuration["DefaultLinuxPermissionsOnStoreCreation"]; } }
        public FileTransferProtocolEnum FileTransferProtocol 
        { 
            get 
            {
                string protocolNames = string.Empty;
                foreach (string protocolName in Enum.GetNames(typeof(FileTransferProtocolEnum)))
                {
                    protocolNames += protocolName + ", ";
                }
                protocolNames = protocolNames.Substring(0, protocolNames.Length - 2);

                FileTransferProtocolEnum protocol;
                if (!Enum.TryParse(configuration["FileTransferProtocol"], out protocol))
                    throw new RemoteFileException($"Invalid optional config.json FileTransferProtocol option of {configuration["FileTransferProtocol"]}.  If present, must be one of these values: {protocolNames}.");
                return protocol; 
            }
        }

        public static void Initialize(string configLocation)
        {
            ConfigurationBuilder configBuilder = (ConfigurationBuilder)new ConfigurationBuilder().SetBasePath(configLocation).AddJsonFile("config.Json", optional: false, reloadOnChange: true);
            configuration = configBuilder.Build();
        }
    }
}
