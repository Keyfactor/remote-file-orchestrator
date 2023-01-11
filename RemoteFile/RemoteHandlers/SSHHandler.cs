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

using Keyfactor.Logging;
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
            _logger.MethodEntry(LogLevel.Debug);
            
            Server = server;

            List<AuthenticationMethod> authenticationMethods = new List<AuthenticationMethod>();
            if (serverPassword.Length < PASSWORD_LENGTH_MAX)
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(serverLogin, serverPassword));
            }
            else
            {
                PrivateKeyFile privateKeyFile;

                try
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(FormatRSAPrivateKey(serverPassword))))
                    {
                        privateKeyFile = new PrivateKeyFile(ms);
                    }
                }
                catch (Exception ex)
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(ConvertToPKCS1(serverPassword))))
                    {
                        privateKeyFile = new PrivateKeyFile(ms);
                    }
                }

                //RsaSha256Util.ConvertToKeyWithSha256Signature(privateKeyFile);
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(serverLogin, privateKeyFile));
            }

            Connection = new ConnectionInfo(server, serverLogin, authenticationMethods.ToArray());
            //RsaSha256Util.SetupConnection(Connection);
            
            _logger.MethodExit(LogLevel.Debug);
        }

        public override void Initialize()
        {
            _logger.MethodEntry(LogLevel.Debug);

            try
            {
                sshClient = new SshClient(Connection);
                sshClient.Connect();
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error making a SSH connection to remote server {Connection.Host}, for user {Connection.Username}.  Please contact your company's system administrator to verify connection and permission settings.", ex);
            }
            
            _logger.MethodExit(LogLevel.Debug);
        }

        public override void Terminate()
        {
            _logger.MethodEntry(LogLevel.Debug);
            
            sshClient.Disconnect();
            sshClient.Dispose();
            
            _logger.MethodExit(LogLevel.Debug);
        }

        public override string RunCommand(string commandText, object[] arguments, bool withSudo, string[] passwordsToMaskInLog)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"RunCommand: {commandText}");

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

                    if (!String.IsNullOrEmpty(command.Error))
                        throw new ApplicationException(command.Error);

                    _logger.MethodExit(LogLevel.Debug);

                    return command.Result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during RunCommand...{RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw ex;
            }
        }

        public override void UploadCertificateFile(string path, string fileName, byte[] certBytes)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"UploadCertificateFile: {path}{fileName}");

            string uploadPath = path+fileName;

            if (!string.IsNullOrEmpty(ApplicationSettings.SeparateUploadFilePath))
            {
                uploadPath = ApplicationSettings.SeparateUploadFilePath + fileName;
            }

            bool scpError = false;

            if (ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both || ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.SCP)
            {
                using (ScpClient client = new ScpClient(Connection))
                {
                    try
                    {
                        _logger.LogDebug($"SCP connection attempt to {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}");
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream(certBytes))
                        {
                            client.Upload(stream, FormatFTPPath(uploadPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        scpError = true;
                        _logger.LogError("Exception during SCP upload...");
                        _logger.LogError($"Upload Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                        if (ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both)
                            _logger.LogDebug($"SCP upload failed.  Attempting with SFTP protocol...");
                        else
                            throw new RemoteFileException("Error attempting SCP file transfer to {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}.  Please contact your company's system administrator to verify connection and permission settings.", ex);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }

            if ((ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both && scpError) || ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.SFTP)
            {
                using (SftpClient client = new SftpClient(Connection))
                {
                    try
                    {
                        _logger.LogDebug($"SFTP connection attempt to {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}");
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream(certBytes))
                        {
                            client.UploadFile(stream, FormatFTPPath(uploadPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception during SFTP upload...");
                        _logger.LogError($"Upload Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                        throw new RemoteFileException("Error attempting SFTP file transfer to {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}.  Please contact your company's system administrator to verify connection and permission settings.", ex);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }

            if (!string.IsNullOrEmpty(ApplicationSettings.SeparateUploadFilePath))
            {
                RunCommand($"cp -a {uploadPath} {path}", null, ApplicationSettings.UseSudo, null);
                RunCommand($"rm {uploadPath}", null, ApplicationSettings.UseSudo, null);
            }

            _logger.MethodExit(LogLevel.Debug);
        }

        public override byte[] DownloadCertificateFile(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DownloadCertificateFile: {path}");

            byte[] rtnStore = new byte[] { };

            string downloadPath = path;
            string altPathOnly = string.Empty;
            string altFileNameOnly = string.Empty;

            if (!string.IsNullOrEmpty(ApplicationSettings.SeparateUploadFilePath))
            {
                SplitStorePathFile(path, out altPathOnly, out altFileNameOnly);
                downloadPath = ApplicationSettings.SeparateUploadFilePath + altFileNameOnly;
                RunCommand($"cp {path} {downloadPath}", null, ApplicationSettings.UseSudo, null);
                RunCommand($"chown {Connection.Username} {downloadPath}", null, ApplicationSettings.UseSudo, null);
            }

            bool scpError = false;

            if (ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both || ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.SCP)
            {
                using (ScpClient client = new ScpClient(Connection))
                {
                    try
                    {
                        _logger.LogDebug($"SCP connection attempt from {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}");
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            client.Download(FormatFTPPath(downloadPath), stream);
                            rtnStore = stream.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        scpError = true;
                        _logger.LogError("Exception during SCP download...");
                        _logger.LogError($"Upload Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                        if (ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both)
                            _logger.LogDebug($"SCP download failed.  Attempting with SFTP protocol...");
                        else
                            throw new RemoteFileException($"Error attempting SCP file transfer from {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}.  Please contact your company's system administrator to verify connection and permission settings.", ex);
                    }
                    finally
                    {
                        client.Disconnect();
                    }
                }
            }

            if ((ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.Both && scpError) || ApplicationSettings.FileTransferProtocol == ApplicationSettings.FileTransferProtocolEnum.SFTP)
            {
                using (SftpClient client = new SftpClient(Connection))
                {
                    try
                    {
                        _logger.LogDebug($"SFTP connection attempt from {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}");
                        client.Connect();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            client.DownloadFile(FormatFTPPath(downloadPath), stream);
                            rtnStore = stream.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception during SFTP download...");
                        _logger.LogError($"Download Exception: {RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                        throw new RemoteFileException($"Error attempting SFTP file transfer from {Connection.Host} using login {Connection.Username} and connection method {Connection.AuthenticationMethods[0].Name}.  Please contact your company's system administrator to verify connection and permission settings.", ex);
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

            _logger.MethodExit(LogLevel.Debug);

            return rtnStore;
        }

        public override void CreateEmptyStoreFile(string path, string linuxFilePermissions, string linuxFileOwner)
        {
            _logger.MethodEntry(LogLevel.Debug);

            AreLinuxPermissionsValid(linuxFilePermissions);
            RunCommand($"install -m {linuxFilePermissions} -o {linuxFileOwner} -g {linuxFileOwner} /dev/null {path}", null, ApplicationSettings.UseSudo, null);

            _logger.MethodExit(LogLevel.Debug);
        }

        public override bool DoesFileExist(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DoesFileExist: {path}");

            using (SftpClient client = new SftpClient(Connection))
            {
                try
                {
                    client.Connect();
                    string existsPath = FormatFTPPath(path);
                    bool exists = client.Exists(existsPath);

                    _logger.MethodExit(LogLevel.Debug);

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
                throw new RemoteFileException($"Invalid format for Linux file permissions.  This value must be exactly 3 digits long with each digit between 0-7 but found {permissions} instead.");
        }

        public override void RemoveCertificateFile(string path, string fileName)
        {
            _logger.LogDebug($"RemoveCertificateFile: {path} {fileName}");

            RunCommand($"rm {path}{fileName}", null, ApplicationSettings.UseSudo, null);
        }

        private void SplitStorePathFile(string pathFileName, out string path, out string fileName)
        {
            _logger.MethodEntry(LogLevel.Debug);

            try
            {
                int separatorIndex = pathFileName.LastIndexOf(pathFileName.Substring(0, 1) == "/" ? @"/" : @"\");
                fileName = pathFileName.Substring(separatorIndex + 1);
                path = pathFileName.Substring(0, separatorIndex + 1);
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to parse certficate store/key path={pathFileName}.", ex);
            }

            _logger.MethodEntry(LogLevel.Debug);
        }

        private string FormatRSAPrivateKey(string privateKey)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.MethodExit(LogLevel.Debug);
            
            return privateKey.Replace(" RSA PRIVATE ", "^^^").Replace(" ", System.Environment.NewLine).Replace("^^^", " RSA PRIVATE ") + System.Environment.NewLine;
        }

        private string ConvertToPKCS1(string privateKey)
        {
            _logger.MethodEntry(LogLevel.Debug);

            privateKey = privateKey.Replace(System.Environment.NewLine, string.Empty).Replace("-----BEGIN PRIVATE KEY-----", string.Empty).Replace("-----END PRIVATE KEY-----", string.Empty);
            PrivateKeyConverter conv = PrivateKeyConverterFactory.FromPkcs8Blob(Convert.FromBase64String(privateKey), string.Empty);
            RSA alg = (RSA)conv.ToNetPrivateKey();
            string pemString = PemUtilities.DERToPEM(alg.ExportRSAPrivateKey(), PemUtilities.PemObjectType.PrivateKey);

            _logger.MethodExit(LogLevel.Debug);

            return pemString.Replace("PRIVATE", "RSA PRIVATE");
        }

        private string FormatFTPPath(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.MethodExit(LogLevel.Debug);

            return path.Substring(0, 1) == @"/" ? path : @"/" + path.Replace("\\", "/");
        }
    }
}
