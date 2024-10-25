// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.IO;
using System.Security.Cryptography;

using CliWrap;
using CliWrap.Buffered;

using Renci.SshNet;

using Microsoft.Extensions.Logging;

using Keyfactor.Logging;
using Keyfactor.PKI.PrivateKeys;
using Keyfactor.PKI.PEM;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    class LinuxLocalHandler : BaseRemoteHandler
    {
        private Command BaseCommand { get; set; }

        internal LinuxLocalHandler()
        {
            _logger.MethodEntry(LogLevel.Debug);

            BaseCommand = Cli.Wrap("/bin/bash");

            _logger.MethodExit(LogLevel.Debug);
        }

        public override void Terminate()
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.MethodExit(LogLevel.Debug);
        }

        public override string RunCommand(string commandText, object[] arguments, bool withSudo, string[] passwordsToMaskInLog)
        {
            _logger.MethodEntry(LogLevel.Debug);

            string sudo = $"echo -e \\n | sudo -S ";

            try
            {
                if (withSudo)
                    commandText = sudo + commandText;

                string displayCommand = commandText;
                if (passwordsToMaskInLog != null)
                {
                    foreach (string password in passwordsToMaskInLog)
                        displayCommand = displayCommand.Replace(password, PASSWORD_MASK_VALUE);
                }

                _logger.LogDebug($"RunCommand: {displayCommand}");

                Command cmd = BaseCommand.WithArguments($@"-c ""{commandText}""");
                BufferedCommandResult result = cmd.ExecuteBufferedAsync().GetAwaiter().GetResult();

                _logger.LogDebug($"Linux Local Results: {displayCommand}::: {result.StandardOutput}::: {result.StandardError}");

                if (!String.IsNullOrEmpty(result.StandardError))
                    throw new ApplicationException(result.StandardError);

                _logger.MethodExit(LogLevel.Debug);

                return result.StandardOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during RunCommand...{RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw;
            }
        }

        public override void UploadCertificateFile(string path, string fileName, byte[] certBytes)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"UploadCertificateFile: {path}{fileName}");

            string uploadPath = path+fileName;
            
            try
            {
                File.WriteAllBytes(uploadPath, certBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error attempting upload file to {uploadPath}...");
                _logger.LogError($"Upload Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw new RemoteFileException($"Error attempting upload file to {uploadPath}.", ex);
            }

            _logger.MethodExit(LogLevel.Debug);
        }

        public override byte[] DownloadCertificateFile(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DownloadCertificateFile: {path}");

            byte[] rtnStore = new byte[] { };

            try
            {
                rtnStore = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error attempting download file {path}...");
                _logger.LogError($"Download Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw new RemoteFileException($"Error attempting download file {path}.", ex);
            }

            _logger.MethodExit(LogLevel.Debug);

            return rtnStore;
        }

        public override void CreateEmptyStoreFile(string path, string linuxFilePermissions, string linuxFileOwner)
        {
            _logger.MethodEntry(LogLevel.Debug);
            string[] linuxGroupOwner = linuxFileOwner.Split(":");
            string linuxFileGroup = linuxFileOwner;

            if (linuxGroupOwner.Length == 2)
            {
                linuxFileOwner = linuxGroupOwner[0];
                linuxFileGroup = linuxGroupOwner[1];
            }

            AreLinuxPermissionsValid(linuxFilePermissions);
            RunCommand($"install -m {linuxFilePermissions} -o {linuxFileOwner} -g {linuxFileGroup} /dev/null {path}", null, ApplicationSettings.UseSudo, null);

            _logger.MethodExit(LogLevel.Debug);
        }

        public override bool DoesFileExist(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DoesFileExist: {path}");

            return File.Exists(path);
        }

        public override void RemoveCertificateFile(string path, string fileName)
        {
            _logger.LogDebug($"RemoveCertificateFile: {path} {fileName}");

            RunCommand($"rm {path}{fileName}", null, ApplicationSettings.UseSudo, null);
        }
    }
}
