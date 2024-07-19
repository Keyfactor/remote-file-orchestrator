// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.Logging;

using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    abstract class BaseRemoteHandler : IRemoteHandler
    {
        internal ILogger _logger;
        internal const string PASSWORD_MASK_VALUE = "[PASSWORD]";
        internal const int PASSWORD_LENGTH_MAX = 100;
        internal const string LINUX_PERMISSION_REGEXP = "^[0-7]{3}$";

        public string Server { get; set; }

        public BaseRemoteHandler()
        {
            _logger = LogHandler.GetClassLogger(this.GetType());
        }

        public static void AreLinuxPermissionsValid(string permissions)
        {
            Regex regex = new Regex(LINUX_PERMISSION_REGEXP);
            if (!regex.IsMatch(permissions))
                throw new RemoteFileException($"Invalid format for Linux file permissions.  This value must be exactly 3 digits long with each digit between 0-7 but found {permissions} instead.");
        }

        public abstract void Terminate();

        public abstract string RunCommand(string commandText, object[] arguments, bool withSudo, string[] passwordsToMaskInLog);

        public abstract void UploadCertificateFile(string path, string fileName, byte[] certBytes);

        public abstract byte[] DownloadCertificateFile(string path);

        public abstract void CreateEmptyStoreFile(string path, string linuxFilePermissions, string linuxFileOwner);

        public abstract bool DoesFileExist(string path);

        public abstract void RemoveCertificateFile(string path, string fileName);
    }
}
