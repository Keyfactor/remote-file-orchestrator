// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Keyfactor.Logging;
using System.Reflection;


namespace Keyfactor.Extensions.Orchestrator.RemoteFile 
{
    public class ApplicationSettings
    {
        public enum FileTransferProtocolEnum
        {
            SCP,
            SFTP,
            Both
        }

        private const string DEFAULT_LINUX_PERMISSION_SETTING = "";
        private const string DEFAULT_OWNER_SETTING = "";
        private const string DEFAULT_SUDO_IMPERSONATION_SETTING = "";
        private const int DEFAULT_SSH_PORT = 22;

        private static Dictionary<string,string> configuration;

        public static bool UseSudo { get { return configuration.ContainsKey("UseSudo") ? configuration["UseSudo"]?.ToUpper() == "Y" : false;  } }
        public static bool CreateStoreIfMissing { get { return configuration.ContainsKey("CreateStoreIfMissing") ? configuration["CreateStoreIfMissing"]?.ToUpper() == "Y" : false; } }
        public static bool UseNegotiate { get { return configuration.ContainsKey("UseNegotiate") ? configuration["UseNegotiate"]?.ToUpper() == "Y" : false; } }
        public static string SeparateUploadFilePath { get { return configuration.ContainsKey("SeparateUploadFilePath") ? AddTrailingSlash(configuration["SeparateUploadFilePath"]) : string.Empty; } }
        public static string DefaultLinuxPermissionsOnStoreCreation { get { return configuration.ContainsKey("DefaultLinuxPermissionsOnStoreCreation") ? configuration["DefaultLinuxPermissionsOnStoreCreation"] : DEFAULT_LINUX_PERMISSION_SETTING; } }
        public static string DefaultOwnerOnStoreCreation { get { return configuration.ContainsKey("DefaultOwnerOnStoreCreation") ? configuration["DefaultOwnerOnStoreCreation"] : DEFAULT_OWNER_SETTING; } }
        public static string DefaultSudoImpersonatedUser { get { return configuration.ContainsKey("DefaultSudoImpersonatedUser") ? configuration["DefaultSudoImpersonatedUser"] : DEFAULT_SUDO_IMPERSONATION_SETTING; } }
        public static bool CreateCSROnDevice { get { return configuration.ContainsKey("CreateCSROnDevice") ? configuration["CreateCSROnDevice"]?.ToUpper() == "Y" : false; } }
        public static string TempFilePathForODKG { get { return configuration.ContainsKey("TempFilePathForODKG") ? configuration["TempFilePathForODKG"] : string.Empty; } }
        public static bool UseShellCommands { get { return configuration.ContainsKey("UseShellCommands") ? configuration["UseShellCommands"]?.ToUpper() == "Y" : true; } }
        public static int SSHPort
        {
            get
            {
                if (configuration.ContainsKey("SSHPort") && !string.IsNullOrEmpty(configuration["SSHPort"]))
                {
                    int sshPort;
                    if (int.TryParse(configuration["SSHPort"], out sshPort))
                        return sshPort;
                    else
                        throw new RemoteFileException($"Invalid optional config.json SSHPort value of {configuration["SSHPort"]}.  If present, this must be an integer value.");
                }
                else
                {
                    return DEFAULT_SSH_PORT;
                }
            }
        }
        public static FileTransferProtocolEnum FileTransferProtocol 
        { 
            get 
            {
                ILogger logger = LogHandler.GetClassLogger<ApplicationSettings>();
                
                string protocolNames = string.Empty;
                foreach (string protocolName in Enum.GetNames(typeof(FileTransferProtocolEnum)))
                {
                    protocolNames += protocolName + ", ";
                }
                protocolNames = protocolNames.Substring(0, protocolNames.Length - 2);
                string? protocolValue = configuration["FileTransferProtocol"];

                if (!PropertyUtilities.TryEnumParse(protocolValue, out bool isFlagCombination, out FileTransferProtocolEnum protocol))
                    throw new RemoteFileException($"Invalid optional config.json FileTransferProtocol option of {protocolValue}.  If present, must be one of these values: {protocolNames}.");

                // Issue: If received a comma-delimited list ("SCP,SFTP,Both"), it's treating it as a flag combination (i.e. mapping it to 0+1+2=3)
                // If this happens, we want to default it to Both so it's resolved as a valid mapping.
                if (isFlagCombination)
                {
                    logger.LogWarning($"FileTransferProtocol config value {protocolValue} mapped to a flag combination. Setting FileTransferProtocol explicitly to Both.");
                    protocol = FileTransferProtocolEnum.Both;
                }
                
                return protocol; 
            }
        }

        static ApplicationSettings()
        {
            ILogger logger = LogHandler.GetClassLogger<ApplicationSettings>();
            logger.MethodEntry(LogLevel.Debug);

            configuration = new Dictionary<string, string>();
            string configLocation = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}config.json";
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
            if (!configuration.ContainsKey("DefaultOwnerOnStoreCreation"))
                logger.LogDebug($"Missing configuration parameter - DefaultOwnerOnStoreCreation.  Will set to default value of '{DEFAULT_OWNER_SETTING}'");
            if (!configuration.ContainsKey("FileTransferProtocol"))
                logger.LogDebug($"Missing configuration parameter - FileTransferProtocol.  Will set to default value of 'SCP'");
        }

        private static string AddTrailingSlash(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Substring(path.Length - 1, 1) == @"/" ? path : path += @"/";
        }
    }
}
