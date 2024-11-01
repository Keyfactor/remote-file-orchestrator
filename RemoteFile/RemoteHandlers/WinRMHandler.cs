// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Text;

using Microsoft.Extensions.Logging;

using Keyfactor.Logging;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    class WinRMHandler : BaseRemoteHandler
    {
        private const string IGNORED_ERROR1 = "setupcmdline.bat";
        private const string IGNORED_ERROR2 = "operable program or batch file";

        private Runspace runspace { get; set; }
        private WSManConnectionInfo connectionInfo { get; set; }
        private bool RunLocal { get; set; }

        internal WinRMHandler(string server, string serverLogin, string serverPassword, bool treatAsLocal)
        {
            _logger.MethodEntry(LogLevel.Debug);

            Server = server;
            RunLocal = Server.ToLower() == "localhost" || treatAsLocal;

            if (!RunLocal)
            {
                connectionInfo = new WSManConnectionInfo(new System.Uri($"{Server}/wsman"));
                if (!string.IsNullOrEmpty(serverLogin))
                {
                    connectionInfo.Credential = new PSCredential(serverLogin, new NetworkCredential(serverLogin, serverPassword).SecurePassword);
                }
            }

            try
            {
                if (RunLocal)
                {
                    runspace = RunspaceFactory.CreateRunspace();
                }
                else
                {
                    if (ApplicationSettings.UseNegotiate)
                    {
                        connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Negotiate;
                    }
                    runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                }
                runspace.Open();
            }

            catch (Exception ex)
            {
                _logger.LogError($"Exception attempting to connect to server...{RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw;
            }

            _logger.MethodExit(LogLevel.Debug);
        }

        public override void Terminate()
        {
            _logger.MethodEntry(LogLevel.Debug);

            runspace.Close();
            runspace.Dispose();

            _logger.MethodExit(LogLevel.Debug);
        }

        public override string RunCommand(string commandText, object[] parameters, bool withSudo, string[] passwordsToMaskInLog)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"RunCommand: {commandText}");

            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript(commandText);

                    string displayCommand = commandText;
                    if (passwordsToMaskInLog != null)
                    {
                        foreach (string password in passwordsToMaskInLog)
                            displayCommand = displayCommand.Replace(password, PASSWORD_MASK_VALUE);
                    }

                    if (parameters != null)
                    {
                        foreach (object parameter in parameters)
                            ps.AddArgument(parameter);
                    }

                    _logger.LogDebug($"RunCommand: {displayCommand}");
                    string result = FormatResult(ps.Invoke(parameters));

                    if (ps.HadErrors)
                    {
                        string errors = string.Empty;
                        System.Collections.ObjectModel.Collection<ErrorRecord> errorRecords = ps.Streams.Error.ReadAll();
                        foreach (ErrorRecord errorRecord in errorRecords)
                        {
                            string error = errorRecord.ToString();
                            if (error.ToLower().Contains(IGNORED_ERROR1)
                             || error.ToLower().Contains(IGNORED_ERROR2))
                            {
                                errors = null;
                                break;
                            }

                            errors += (error + "   ");
                        }

                        if (!string.IsNullOrEmpty(errors))
                            throw new ApplicationException(errors);
                    }
                    else
                        _logger.LogDebug($"WinRM Results: {displayCommand}::: {result}");

                    _logger.MethodExit(LogLevel.Debug);

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception during RunCommand...{RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw;
            }
        }

        private byte[] RunCommandBinary(string commandText)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"RunCommandBinary: {commandText}");

            byte[] rtnBytes = new byte[0];

            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript(commandText);

                    System.Collections.ObjectModel.Collection<PSObject> psResult = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        string errors = string.Empty;
                        System.Collections.ObjectModel.Collection<ErrorRecord> errorRecords = ps.Streams.Error.ReadAll();
                        foreach (ErrorRecord errorRecord in errorRecords)
                            errors += (errorRecord.ToString() + "   ");

                        throw new ApplicationException(errors);
                    }
                    else
                    {
                        if (psResult.Count > 0)
                            rtnBytes = (byte[])psResult[0].BaseObject;
                        _logger.LogDebug($"WinRM Results: {commandText}::: binary results.");
                    }
                }

                _logger.MethodExit(LogLevel.Debug);

                return rtnBytes;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Exception during RunCommandBinary...{RemoteFileException.FlattenExceptionMessages(ex, ex.Message)}");
                throw;
            }
        }

        public override void UploadCertificateFile(string path, string fileName, byte[] certBytes)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"UploadCertificateFile: {path} {fileName}");

            string cmdOption = RunCommand($@"$PSVersionTable.PSEdition", null, false, null).ToLower().Contains("core") ? "AsByteStream" : "Encoding Byte";

            string scriptBlock = $@"
                                    param($contents)
                                
                                    Set-Content ""{path + fileName}"" -{cmdOption} -Value $contents
                                ";

            object[] arguments = new object[] { certBytes };

            RunCommand(scriptBlock, arguments, false, null);

            _logger.MethodEntry(LogLevel.Debug);
        }

        public override byte[] DownloadCertificateFile(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DownloadCertificateFile: {path}");
            _logger.MethodExit(LogLevel.Debug);

            string cmdOption = RunCommand($@"$PSVersionTable.PSEdition", null, false, null).ToLower().Contains("core") ? "AsByteStream" : "Encoding Byte";

            return RunCommandBinary($@"Get-Content -Path ""{path}"" -{cmdOption} -Raw");
        }

        public override void CreateEmptyStoreFile(string path, string linuxFilePermissions, string linuxFileOwner)
        {
            _logger.MethodEntry(LogLevel.Debug);
            RunCommand($@"Out-File -FilePath ""{path}""", null, false, null);
            _logger.MethodExit(LogLevel.Debug);
        }

        public override bool DoesFileExist(string path)
        {
            _logger.MethodEntry(LogLevel.Debug);
            _logger.LogDebug($"DoesFileExist: {path}");
            _logger.MethodExit(LogLevel.Debug);

            return Convert.ToBoolean(RunCommand($@"Test-Path -path ""{path}""", null, false, null));
        }

        public override void RemoveCertificateFile(string path, string fileName)
        {
            _logger.LogDebug($"RemoveCertificateFile: {path} {fileName}");

            RunCommand($"rm {path}{fileName}", null, false, null);
        }


        private string FormatResult(ICollection<PSObject> results)
        {
            _logger.MethodEntry(LogLevel.Debug);

            StringBuilder rtn = new StringBuilder();

            foreach (PSObject resultLine in results)
            {
                if (resultLine != null)
                    rtn.Append(resultLine.ToString() + System.Environment.NewLine);
            }

            _logger.MethodExit(LogLevel.Debug);

            return rtn.ToString();
        }
    }
}
