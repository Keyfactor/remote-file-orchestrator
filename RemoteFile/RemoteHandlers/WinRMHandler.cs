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

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    class WinRMHandler : BaseRemoteHandler
    {
        private const string IGNORED_ERROR1 = "importing keystore";
        private const string IGNORED_ERROR2 = "warning:";
        private const string IGNORED_ERROR3 = "certificate was added to keystore";

        private Runspace runspace { get; set; }
        private WSManConnectionInfo connectionInfo { get; set; }

        internal WinRMHandler(string server, string serverLogin, string serverPassword)
        {
            Server = server;
            connectionInfo = new WSManConnectionInfo(new System.Uri($"{Server}/wsman"));
            if (!string.IsNullOrEmpty(serverLogin))
            {
                connectionInfo.Credential = new PSCredential(serverLogin, new NetworkCredential(serverLogin, serverPassword).SecurePassword);
            }
        }

        public override void Initialize()
        {
            try
            {
                runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();
            }

            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during Initialize...{ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                throw ex;
            }
        }

        public override void Terminate()
        {
            runspace.Close();
            runspace.Dispose();
        }

        public override string RunCommand(string commandText, object[] parameters, bool withSudo, string[] passwordsToMaskInLog)
        {
            _logger.LogDebug($"RunCommand: {Server}");

            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;

                    if (commandText.ToLower().IndexOf("keytool ") > -1)
                    {
                        commandText = ($"& '{commandText}").Replace("keytool", "keytool'");
                        commandText = "echo '' | " + commandText;
                    }
                    ps.AddScript(commandText);

                    string displayCommand = commandText;
                    if (passwordsToMaskInLog != null)
                    {
                        foreach (string password in passwordsToMaskInLog)
                            displayCommand = displayCommand.Replace(password, PASSWORD_MASK_VALUE);
                    }

                    if (parameters != null)
                    {
                        foreach(object parameter in parameters)
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
                            if (error.ToLower().StartsWith(IGNORED_ERROR1) ||
                                error.ToLower().Contains(IGNORED_ERROR2) ||
                                error.ToLower().Contains(IGNORED_ERROR3))
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

                    if (result.ToLower().Contains(KEYTOOL_ERROR))
                        throw new ApplicationException(result);

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during RunCommand...{ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                throw ex;
            }
        }

        private byte[] RunCommandBinary(string commandText)
        {
            _logger.LogDebug($"RunCommandBinary: {Server}");
            byte[] rtnBytes = new byte[0];

            try
            {
                using (Runspace runspace = RunspaceFactory.CreateRunspace(new WSManConnectionInfo(new System.Uri($"{Server}/wsman"))))
                {
                    runspace.Open();
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;
                        ps.AddScript(commandText);

                        _logger.LogDebug($"RunCommandBinary: {commandText}");
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
                }

                return rtnBytes;
            }

            catch (Exception ex)
            {
                _logger.LogDebug("Exception during RunCommandBinary...{ExceptionHandler.FlattenExceptionMessages(ex, ex.Message)}");
                throw ex;
            }
        }

        public override void UploadCertificateFile(string path, string fileName, byte[] certBytes)
        {
            _logger.LogDebug($"UploadCertificateFile: {path} {fileName}");

            string scriptBlock = $@"
                                    param($contents)
                                
                                    Set-Content {path + fileName} -Encoding Byte -Value $contents
                                ";

            object[] arguments = new object[] { certBytes };

            RunCommand(scriptBlock, arguments, false, null);
        }

        public override byte[] DownloadCertificateFile(string path)
        {
            _logger.LogDebug($"DownloadCertificateFile: {path}");
            return RunCommandBinary($@"Get-Content -Path ""{path}"" -Encoding Byte -Raw");
        }

        public override void CreateEmptyStoreFile(string path, string linuxFilePermissions)
        {
            RunCommand($@"Out-File -FilePath ""{path}""", null, false, null);
        }

        public override bool DoesFileExist(string path)
        {
            _logger.LogDebug($"DoesFileExist: {path}");

            return Convert.ToBoolean(RunCommand($@"Test-Path -path ""{path}""", null, false, null));
        }


        private string FormatResult(ICollection<PSObject> results)
        {
            StringBuilder rtn = new StringBuilder();

            foreach (PSObject resultLine in results)
            {
                if (resultLine != null)
                    rtn.Append(resultLine.ToString() + System.Environment.NewLine);
            }

            return rtn.ToString();
        }

        private string FormatFTPPath(string path)
        {
            return path.Substring(0, 1) == @"/" ? path : @"/" + path.Replace("\\", "/");
        }
    }
}
