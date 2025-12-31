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
using Microsoft.PowerShell;


namespace Keyfactor.Extensions.Orchestrator.RemoteFile 
{
    public class ApplicationSettings
    {
        private const string DEFAULT_LINUX_PERMISSION_SETTING = "";
        private const string DEFAULT_OWNER_SETTING = "";
        private const string DEFAULT_SUDO_IMPERSONATION_SETTING = "";
        private const int DEFAULT_SSH_PORT = 22;

        private static Dictionary<string,object> configuration;

        public static bool UseSudo { get { return configuration.ContainsKey("UseSudo") ? configuration["UseSudo"]?.ToString().ToUpper() == "Y" : false;  } }
        public static bool CreateStoreIfMissing { get { return configuration.ContainsKey("CreateStoreIfMissing") ? configuration["CreateStoreIfMissing"]?.ToString().ToUpper() == "Y" : false; } }
        public static bool UseNegotiate { get { return configuration.ContainsKey("UseNegotiate") ? configuration["UseNegotiate"]?.ToString().ToUpper() == "Y" : false; } }
        public static string SeparateUploadFilePath { get { return configuration.ContainsKey("SeparateUploadFilePath") ? AddTrailingSlash(configuration["SeparateUploadFilePath"].ToString()) : string.Empty; } }
        public static string DefaultLinuxPermissionsOnStoreCreation { get { return configuration.ContainsKey("DefaultLinuxPermissionsOnStoreCreation") ? configuration["DefaultLinuxPermissionsOnStoreCreation"].ToString() : DEFAULT_LINUX_PERMISSION_SETTING; } }
        public static string DefaultOwnerOnStoreCreation { get { return configuration.ContainsKey("DefaultOwnerOnStoreCreation") ? configuration["DefaultOwnerOnStoreCreation"].ToString() : DEFAULT_OWNER_SETTING; } }
        public static string DefaultSudoImpersonatedUser { get { return configuration.ContainsKey("DefaultSudoImpersonatedUser") ? configuration["DefaultSudoImpersonatedUser"].ToString() : DEFAULT_SUDO_IMPERSONATION_SETTING; } }
        public static string TempFilePathForODKG { get { return configuration.ContainsKey("TempFilePathForODKG") ? configuration["TempFilePathForODKG"].ToString() : string.Empty; } }
        public static bool UseShellCommands { get { return configuration.ContainsKey("UseShellCommands") ? configuration["UseShellCommands"]?.ToString().ToUpper() == "Y" : true; } }
        public static int SSHPort
        {
            get
            {
                if (configuration.ContainsKey("SSHPort") && !string.IsNullOrEmpty(configuration["SSHPort"]?.ToString()))
                {
                    int sshPort;
                    if (int.TryParse(configuration["SSHPort"]?.ToString(), out sshPort))
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
        public static List<PostJobCommand> PostJobCommands 
        {
            get
            {
                if (configuration.ContainsKey("PostJobCommands") && configuration["PostJobCommands"] != null)
                {
                    return JsonConvert.DeserializeObject<List<PostJobCommand>>(configuration["PostJobCommands"]?.ToString());
                }
                else
                {
                    return new List<PostJobCommand>();
                }
            }
        }

        static ApplicationSettings()
        {
            ILogger logger = LogHandler.GetClassLogger<ApplicationSettings>();
            logger.MethodEntry(LogLevel.Debug);

            configuration = new Dictionary<string, object>();
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

            try
            {
                configuration = JsonConvert.DeserializeObject<Dictionary<string, object>>(configContents);
            }
            catch (Exception ex)
            {
                throw new RemoteFileException(RemoteFileException.FlattenExceptionMessages(ex, "Error attempting to serialize config.json file.  Please review your config.json file for proper formatting."));
            }
            ValidateConfiguration(logger);

            logger.LogDebug("Configuration Settings:");
            foreach(KeyValuePair<string,object> keyValue in configuration)
            {
                logger.LogDebug($"    {keyValue.Key}: {keyValue.Value}");
            }
            
            logger.MethodExit(LogLevel.Debug);
        }

        private static void ValidateConfiguration(ILogger logger)
        {
            if (!configuration.ContainsKey("UseSudo") || (configuration["UseSudo"]?.ToString().ToUpper() != "Y" && configuration["UseSudo"]?.ToString().ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - UseSudo.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("CreateStoreIfMissing") || (configuration["CreateStoreIfMissing"]?.ToString().ToUpper() != "Y" && configuration["CreateStoreIfMissing"]?.ToString().ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - CreateStoreIfMissing.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("UseNegotiate") || (configuration["UseNegotiate"]?.ToString().ToUpper() != "Y" && configuration["UseNegotiate"]?.ToString().ToUpper() != "N"))
                logger.LogDebug($"Missing or invalid configuration parameter - UseNegotiate.  Will set to default value of 'False'");
            if (!configuration.ContainsKey("SeparateUploadFilePath"))
                logger.LogDebug($"Missing configuration parameter - SeparateUploadFilePath.  Will set to default value of ''");
            if (!configuration.ContainsKey("DefaultLinuxPermissionsOnStoreCreation"))
                logger.LogDebug($"Missing configuration parameter - DefaultLinuxPermissionsOnStoreCreation.  Will set to default value of '{DEFAULT_LINUX_PERMISSION_SETTING}'");
            if (!configuration.ContainsKey("DefaultOwnerOnStoreCreation"))
                logger.LogDebug($"Missing configuration parameter - DefaultOwnerOnStoreCreation.  Will set to default value of '{DEFAULT_OWNER_SETTING}'");
        }

        private static string AddTrailingSlash(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Substring(path.Length - 1, 1) == @"/" ? path : path += @"/";
        }

        public class PostJobCommand
        {
            public string Name { get; set; }
            public string Environment { get; set; }
            public string Command { get; set; }
        }
    }
}
