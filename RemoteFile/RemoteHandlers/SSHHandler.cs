// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

using Renci.SshNet;

using Microsoft.Extensions.Logging;

using Keyfactor.PKI.PrivateKeys;
using Keyfactor.PKI.PEM;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    class SSHHandler : BaseRemoteHandler
    {
        private const string LINUX_PERMISSION_REGEXP = "^[0-7]{3}$";
        private ConnectionInfo Connection { get; set; }

        private SshClient sshClient;

        internal SSHHandler(string server, string serverLogin, string serverPassword)
        {
            Server = server;

            List<AuthenticationMethod> authenticationMethods = new List<AuthenticationMethod>();
            if (serverPassword.Length < PASSWORD_LENGTH_MAX)
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(serverLogin, serverPassword));
            }
            else
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(FormatRSAPrivateKey(serverPassword))))
                    {
                        authenticationMethods.Add(new PrivateKeyAuthenticationMethod(serverLogin, new PrivateKeyFile[] { new PrivateKeyFile(ms) }));
                    }
                }
                catch (Exception ex)
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(ConvertToPKCS1(serverPassword))))
                    {
                        authenticationMethods.Add(new PrivateKeyAuthenticationMethod(serverLogin, new PrivateKeyFile[] { new PrivateKeyFile(ms) }));
                    }
                }

            }

            Connection = new ConnectionInfo(server, serverLogin, authenticationMethods.ToArray());
        }

        public override void Initialize()
        {
            sshClient = new SshClient(Connection);
            sshClient.Connect();
        }

        public override void Terminate()
        {
            sshClient.Disconnect();
            sshClient.Dispose();
        }

        public override string RunCommand(string commandText, object[] arguments, bool withSudo, string[] passwordsToMaskInLog)
        {
            _logger.LogDebug($"RunCommand: {Server}");

            string sudo = $"sudo -i -S ";
            string echo = $"echo -e '\n' | ";

            try
            {
                if (withSudo)
                    commandText = sudo + commandText;

                commandText = echo + commandText;

                string displayCommand = commandText;
                if (passwordsToMaskInLog != null)
                {
                    foreach (string password in passwordsToMaskInLog)
                        displayCommand = displayCommand.Replace(password, PASSWORD_MASK_VALUE);
                }

                using (SshCommand command = sshClient.CreateCommand($"{commandText}"))
                {
                    _logger.LogDebug($"RunCommand: {displayCommand}");
                    command.Execute();
                    _logger.LogDebug($"SSH Results: {displayCommand}::: {command.Result}::: {command.Error}");

                    if (command.Result.ToLower().Contains(KEYTOOL_ERROR))
                        throw new ApplicationException(command.Result);

                    return command.Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during RunCommand...{ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                throw ex;
            }
        }

        public override void UploadCertificateFile(string path, string fileName, byte[] certBytes)
        {
            _logger.LogDebug($"UploadCertificateFile: {path}{fileName}");
            string uploadPath = path+fileName;

            if (ApplicationSettings.UseSeparateUploadFilePath)
            {
                uploadPath = ApplicationSettings.SeparateUploadFilePath + fileName;
            }

            if (ApplicationSettings.UseSCP)
            {
                using (ScpClient client = new ScpClient(Connection))
                {
                    try
                    {
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream(certBytes))
                        {
                            client.Upload(stream, FormatFTPPath(uploadPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("Exception during SCP upload...");
                        _logger.LogDebug($"Upload Exception: {ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                        throw ex;
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }
            else
            {
                using (SftpClient client = new SftpClient(Connection))
                {
                    try
                    {
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream(certBytes))
                        {
                            client.UploadFile(stream, FormatFTPPath(uploadPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("Exception during SFTP upload...");
                        _logger.LogDebug($"Upload Exception: {ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                        throw ex;
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }

            if (ApplicationSettings.UseSeparateUploadFilePath)
            {
                RunCommand($"cp -a {uploadPath} {path}", null, ApplicationSettings.UseSudo, null);
                RunCommand($"rm {uploadPath}", null, ApplicationSettings.UseSudo, null);
            }
        }

        public override byte[] DownloadCertificateFile(string path)
        {
            _logger.LogDebug($"DownloadCertificateFile: {path}");

            byte[] rtnStore;

            string downloadPath = path;
            string altPathOnly = string.Empty;
            string altFileNameOnly = string.Empty;

            if (ApplicationSettings.UseSeparateUploadFilePath)
            {
                SplitStorePathFile(path, out altPathOnly, out altFileNameOnly);
                downloadPath = ApplicationSettings.SeparateUploadFilePath + altFileNameOnly;
                RunCommand($"cp {path} {downloadPath}", null, ApplicationSettings.UseSudo, null);
                RunCommand($"sudo chown {Connection.Username} {path}", null, ApplicationSettings.UseSudo, null);
            }

            if (ApplicationSettings.UseSCP)
            {
                using (ScpClient client = new ScpClient(Connection))
                {
                    try
                    {
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            client.Download(FormatFTPPath(downloadPath), stream);
                            rtnStore = stream.ToArray();
                        }
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }
            else
            {
                using (SftpClient client = new SftpClient(Connection))
                {
                    try
                    {
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            client.DownloadFile(FormatFTPPath(downloadPath), stream);
                            rtnStore = stream.ToArray();
                        }
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }

            if (!string.IsNullOrEmpty(ApplicationSettings.SeparateUploadFilePath))
            {
                RunCommand($"rm {downloadPath}", null, ApplicationSettings.UseSudo, null);
            }

            return rtnStore;
        }

        public override void CreateEmptyStoreFile(string path, string linuxFilePermissions)
        {
            AreLinuxPermissionsValid(linuxFilePermissions);
            RunCommand($"install -m {linuxFilePermissions} /dev/null {path}", null, false, null);
            //using sudo will create as root. set useSudo to false 
            //to ensure ownership is with the credentials configued in the platform
        }

        public override bool DoesFileExist(string path)
        {
            _logger.LogDebug($"DoesFileExist: {path}");

            using (SftpClient client = new SftpClient(Connection))
            {
                try
                {
                    client.Connect();
                    string existsPath = FormatFTPPath(path);
                    bool exists = client.Exists(existsPath);

                    return exists;
                }
                finally
                {
                    client.Disconnect();
                }
            }
        }

        public static void AreLinuxPermissionsValid(string permissions)
        {
            Regex regex = new Regex(LINUX_PERMISSION_REGEXP);
            if (!regex.IsMatch(permissions))
                throw new PKCS12Exception($"Invalid format for Linux file permissions.  This value must be exactly 3 digits long with each digit between 0-7 but found {permissions} instead.");
        }

        private void SplitStorePathFile(string pathFileName, out string path, out string fileName)
        {
            try
            {
                int separatorIndex = pathFileName.LastIndexOf(pathFileName.Substring(0, 1) == "/" ? @"/" : @"\");
                fileName = pathFileName.Substring(separatorIndex + 1);
                path = pathFileName.Substring(0, separatorIndex + 1);
            }
            catch (Exception ex)
            {
                throw new PKCS12Exception($"Error attempting to parse certficate store/key path={pathFileName}.", ex);
            }
        }

        private string FormatRSAPrivateKey(string privateKey)
        {
            return privateKey.Replace(" RSA PRIVATE ", "^^^").Replace(" ", System.Environment.NewLine).Replace("^^^", " RSA PRIVATE ");
        }

        private string ConvertToPKCS1(string privateKey)
        {
            privateKey = privateKey.Replace(System.Environment.NewLine, string.Empty).Replace("-----BEGIN PRIVATE KEY-----", string.Empty).Replace("-----END PRIVATE KEY-----", string.Empty);
            PrivateKeyConverter conv = PrivateKeyConverterFactory.FromPkcs8Blob(Convert.FromBase64String(privateKey), string.Empty);
            RSA alg = (RSA)conv.ToNetPrivateKey();
            string pemString = PemUtilities.DERToPEM(alg.ExportRSAPrivateKey(), PemUtilities.PemObjectType.PrivateKey);
            return pemString.Replace("PRIVATE", "RSA PRIVATE");
        }

        private string FormatFTPPath(string path)
        {
            return path.Substring(0, 1) == @"/" ? path : @"/" + path.Replace("\\", "/");
        }
    }
}
