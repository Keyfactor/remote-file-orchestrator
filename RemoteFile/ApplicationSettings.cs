using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Keyfactor.Logging;


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

        private static Dictionary<string,string> configuration;

        public static bool UseSudo { get { return configuration.ContainsKey("UseSudo") ? configuration["UseSudo"]?.ToUpper() == "Y" : false;  } }
        public static bool CreateStoreIfMissing { get { return configuration.ContainsKey("CreateStoreIfMissing") ? configuration["CreateStoreIfMissing"]?.ToUpper() == "Y" : false; } }
        public static bool UseNegotiate { get { return configuration.ContainsKey("UseNegotiate") ? configuration["UseNegotiate"]?.ToUpper() == "Y" : false; } }
        public static string SeparateUploadFilePath { get { return configuration.ContainsKey("SeparateUploadFilePath") ? AddTrailingSlash(configuration["SeparateUploadFilePath"]) : string.Empty; } }
        public static string DefaultLinuxPermissionsOnStoreCreation { get { return configuration.ContainsKey("DefaultLinuxPermissionsOnStoreCreation") ? configuration["DefaultLinuxPermissionsOnStoreCreation"] : DEFAULT_LINUX_PERMISSION_SETTING; } }
        public static FileTransferProtocolEnum FileTransferProtocol 
        { 
            get 
            {
                string protocolNames = string.Empty;
                foreach (string protocolName in Enum.GetNames(typeof(FileTransferProtocolEnum)))
                {
                    protocolNames += protocolName + ", ";
                }
                protocolNames = protocolNames.Substring(0, protocolNames.Length - 2);

                if (!Enum.TryParse(configuration["FileTransferProtocol"], out FileTransferProtocolEnum protocol))
                    throw new RemoteFileException($"Invalid optional config.json FileTransferProtocol option of {configuration["FileTransferProtocol"]}.  If present, must be one of these values: {protocolNames}.");
                return protocol; 
            }
        }

        public static void Initialize(string configLocation)
        {
            ILogger logger = LogHandler.GetClassLogger<ApplicationSettings>();
            logger.MethodEntry(LogLevel.Debug);

            configuration = new Dictionary<string, string>();
            configLocation = $"{Path.GetDirectoryName(configLocation)}{Path.DirectorySeparatorChar}config.json";
            string configContents = string.Empty;

            if (!File.Exists(configLocation))
            {
                logger.LogDebug("config.json missing.  Default values used for configuration.");
                return;
            }

            using (StreamReader sr = new StreamReader(configLocation))
            {
                configContents = sr.ReadToEnd();
                logger.LogDebug($"Raw config.json contents: {configContents}");
            }

            if (String.IsNullOrEmpty(configContents))
            {
                logger.LogDebug("config.json exists but empty.  Default values used for configuration.");
                return;
            }

            configuration = JsonConvert.DeserializeObject<Dictionary<string, string>>(configContents);
            ValidateConfiguration(logger);

            logger.LogDebug("Configuration Settings:");
            foreach(KeyValuePair<string,string> keyValue in configuration)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }
            
            logger.MethodExit(LogLevel.Debug);
        }

        private static void ValidateConfiguration(ILogger logger)
        {
            if (!configuration.ContainsKey("UseSudo") || (configuration["UseSudo"].ToUpper() != "Y" && configuration["UseSudo"].ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - UseSudo.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("CreateStoreIfMissing") || (configuration["CreateStoreIfMissing"].ToUpper() != "Y" && configuration["CreateStoreIfMissing"].ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - CreateStoreIfMissing.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("UseNegotiate") || (configuration["UseNegotiate"].ToUpper() != "Y" && configuration["UseNegotiate"].ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - UseNegotiate.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("SeparateUploadFilePath"))
                logger.LogDebug($"Missing configuration parameter - SeparateUploadFilePath.  Will set to default value of ''");
            if (!configuration.ContainsKey("DefaultLinuxPermissionsOnStoreCreation"))
                logger.LogDebug($"Missing configuration parameter - DefaultLinuxPermissionsOnStoreCreation.  Will set to default value of '{DEFAULT_LINUX_PERMISSION_SETTING}'");
            if (!configuration.ContainsKey("FileTransferProtocol"))
                logger.LogDebug($"Missing configuration parameter - FileTransferProtocol.  Will set to default value of 'SCP'");
        }

        private static string AddTrailingSlash(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Substring(path.Length - 1, 1) == @"/" ? path : path += @"/";
        }
    }
}
