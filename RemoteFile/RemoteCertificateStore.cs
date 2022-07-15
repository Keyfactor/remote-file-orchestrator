// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    internal class RemoteCertificateStore : Pkcs12Store
    {
        private const string NO_EXTENSION = "noext";
        private const string FULL_SCAN = "fullscan";

        const string BEG_DELIM = "-----BEGIN CERTIFICATE-----";
        const string END_DELIM = "-----END CERTIFICATE-----";
        const string FILE_NAME_REPL = "||FILE_NAME_HERE||";

        static Mutex mutex = new Mutex(false, "ModifyStore");

        internal enum ServerTypeEnum
        {
            Linux,
            Windows
        }

        internal string Server { get; set; }
        internal string ServerId { get; set; }
        internal string ServerPassword { get; set; }
        internal string StorePath { get; set; }
        internal string StoreFileName { get; set; }
        internal string StorePassword { get; set; }
        internal IRemoteHandler SSH { get; set; }
        internal ServerTypeEnum ServerType { get; set; }
        internal List<string> DiscoveredStores { get; set; }

        internal string UploadFilePath { get; set; }

        internal RemoteCertificateStore() { }

        internal RemoteCertificateStore(string server, string serverId, string serverPassword, string storeFileAndPath, string storePassword, Dictionary<string, object> jobProperties)
        {
            Server = server;
            SplitStorePathFile(storeFileAndPath);
            ServerId = serverId;
            ServerPassword = serverPassword ?? string.Empty;
            StorePassword = storePassword;
            ServerType = StorePath.Substring(0, 1) == "/" ? ServerTypeEnum.Linux : ServerTypeEnum.Windows;
            UploadFilePath = ApplicationSettings.UseSeparateUploadFilePath && ServerType == ServerTypeEnum.Linux ? ApplicationSettings.SeparateUploadFilePath : StorePath;

            if (!IsStorePathValid())
            {
                string partialMessage = ServerType == ServerTypeEnum.Windows ? @"'\', ':', " : string.Empty;
                throw new PKCS12Exception($"PKCS12 store path {storeFileAndPath} is invalid.  Only alphanumeric, '.', '/', {partialMessage}'-', and '_' characters are allowed in the store path.");
            }
        }

        internal RemoteCertificateStore(string server, string serverId, string serverPassword, ServerTypeEnum serverType)
        {
            Server = server;
            ServerId = serverId;
            ServerPassword = serverPassword ?? string.Empty;
            ServerType = serverType;
        }

        internal void Initialize()
        {
            if (ServerType == ServerTypeEnum.Linux)
                SSH = new SSHHandler(Server, ServerId, ServerPassword);
            else
                SSH = new WinRMHandler(Server, ServerId, ServerPassword);

            SSH.Initialize();
        }

        internal void Terminate()
        {
            if (SSH != null)
                SSH.Terminate();
        }

        internal List<string> FindStores(string[] paths, string[] extensions, string[] files)
        {
            if (DiscoveredStores != null)
                return DiscoveredStores;

            return ServerType == ServerTypeEnum.Linux ? FindStoresLinux(paths, extensions, files) : FindStoresWindows(paths, extensions, files);
        }

        internal List<X509Certificate2Collection> GetCertificateChains()
        {
            List<X509Certificate2Collection> certificateChains = new List<X509Certificate2Collection>();

            byte[] byteContents = SSH.DownloadCertificateFile(StorePath + StoreFileName);
            if (byteContents.Length < 5)
                return certificateChains;

            Pkcs12Store store = new Pkcs12Store();
            
            using (MemoryStream stream = new MemoryStream(byteContents))
            {
                store = new Pkcs12Store(stream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray());
            }

            foreach(string alias in store.Aliases)
            {
                X509Certificate2Collection chain = new X509Certificate2Collection();
                X509CertificateEntry[] entries;

                if (store.IsKeyEntry(alias))
                {
                    entries = store.GetCertificateChain(alias);
                }
                else
                {
                    X509CertificateEntry entry = store.GetCertificate(alias);
                    entries = new X509CertificateEntry[] { entry };
                }

                foreach(X509CertificateEntry entry in entries)
                {
                    X509Certificate2Ext cert = new X509Certificate2Ext(entry.Certificate.GetEncoded());
                    cert.FriendlyNameExt = alias;
                    chain.Add(cert);
                }

                certificateChains.Add(chain);
            }

            return certificateChains;
        }

        internal void DeleteCertificateByAlias(string alias)
        {
            try
            {
                mutex.WaitOne();
                byte[] byteContents = SSH.DownloadCertificateFile(StorePath + StoreFileName);
                Pkcs12Store store = new Pkcs12Store();

                using (MemoryStream stream = new MemoryStream(byteContents))
                {
                    if (stream.Length == 0)
                    {
                        throw new PKCS12Exception($"Alias {alias} does not exist in certificate store {StorePath + StoreFileName}.");
                    }

                    store = new Pkcs12Store(stream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray());

                    if (!store.ContainsAlias(alias))
                    {
                        throw new PKCS12Exception($"Alias {alias} does not exist in certificate store {StorePath + StoreFileName}.");
                    }

                    store.DeleteEntry(alias);

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        store.Save(outStream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                        SSH.UploadCertificateFile(StorePath, StoreFileName, outStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PKCS12Exception($"Error attempting to remove certficate for store path={StorePath}, file name={StoreFileName}.", ex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        internal void CreateCertificateStore(string storePath, string linuxFilePermissions)
        {
            SSH.CreateEmptyStoreFile(storePath, linuxFilePermissions);
        }

        internal void AddCertificate(string alias, string certificateEntry, bool overwrite, string pfxPassword)
        {
            try
            {
                Pkcs12Store certs = new Pkcs12Store();

                mutex.WaitOne();
                byte[] storeBytes = SSH.DownloadCertificateFile(StorePath + StoreFileName);
                byte[] newCertBytes = Convert.FromBase64String(certificateEntry);

                Pkcs12Store store = new Pkcs12Store();
                Pkcs12Store newEntry = new Pkcs12Store();

                X509Certificate2 cert = new X509Certificate2(newCertBytes, pfxPassword, X509KeyStorageFlags.Exportable);
                byte[] binaryCert = cert.Export(X509ContentType.Pkcs12, pfxPassword);

                using (MemoryStream ms = new MemoryStream(string.IsNullOrEmpty(pfxPassword) ? binaryCert : newCertBytes))
                {
                    newEntry = string.IsNullOrEmpty(pfxPassword) ? new Pkcs12Store(ms, new char[0]) : new Pkcs12Store(ms, pfxPassword.ToCharArray());
                }

                using (MemoryStream stream = new MemoryStream(storeBytes))
                {
                    if (stream.Length > 5)
                        store = new Pkcs12Store(stream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray());

                    if (store.ContainsAlias(alias) && !overwrite)
                    {
                        throw new PKCS12Exception($"Alias {alias} already exists in store {StorePath + StoreFileName} and overwrite is set to False.  Please try again with overwrite set to True if you wish to replace this entry.");
                    }

                    string checkAliasExists = string.Empty;
                    foreach (string newEntryAlias in newEntry.Aliases)
                    {
                        if (!newEntry.IsKeyEntry(newEntryAlias))
                            continue;

                        checkAliasExists = newEntryAlias;

                        store.SetKeyEntry(alias, newEntry.GetKey(newEntryAlias), newEntry.GetCertificateChain(newEntryAlias));
                    }

                    if (string.IsNullOrEmpty(checkAliasExists))
                    {
                        Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(cert);
                        X509CertificateEntry bcEntry = new X509CertificateEntry(bcCert);
                        if (store.ContainsAlias(alias))
                        {
                            store.DeleteEntry(alias);
                        }
                        store.SetCertificateEntry(alias, bcEntry);
                    }

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        store.Save(outStream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                        SSH.UploadCertificateFile(StorePath, StoreFileName, outStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PKCS12Exception($"Error attempting to add certficate for store path={StorePath}, file name={StoreFileName}.", ex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        internal bool DoesStoreExist()
        {
            return SSH.DoesFileExist(StorePath + StoreFileName);
        }

        internal bool IsStorePathValid()
        {
            Regex regex = new Regex(ServerType == ServerTypeEnum.Linux ? $@"^[\d\s\w-_/.]*$" : $@"^[\d\s\w-_/.:\\\\]*$");
            return regex.IsMatch(StorePath + StoreFileName);
        }

        private List<string> FindStoresLinux(string[] paths, string[] extensions, string[] fileNames)
        {
            try
            {
                string concatPaths = string.Join(" ", paths);
                string command = $"find {concatPaths} ";

                foreach (string extension in extensions)
                {
                    foreach (string fileName in fileNames)
                    {
                        command += (command.IndexOf("-iname") == -1 ? string.Empty : "-or ");
                        command += $"-iname '{fileName.Trim()}";
                        if (extension.ToLower() == NO_EXTENSION)
                            command += $"' ! -iname '*.*' ";
                        else
                            command += $".{extension.Trim()}' ";
                    }
                }

                string result = string.Empty;
                if (extensions.Any(p => p.ToLower() != NO_EXTENSION))
                    result = SSH.RunCommand(command, null, ApplicationSettings.UseSudo, null);

                return (result.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            }
            catch (Exception ex)
            {
                throw new PKCS12Exception($"Error attempting to find certificate stores for path={string.Join(" ", paths)}.", ex);
            }
        }

        private List<string> FindStoresWindows(string[] paths, string[] extensions, string[] fileNames)
        {
            List<string> results = new List<string>();
            StringBuilder concatFileNames = new StringBuilder();

            if (paths[0] == FULL_SCAN)
            {
                paths = GetAvailablePaths();
                for (int i = 0; i < paths.Length; i++)
                    paths[i] += "/";
            }

            foreach (string path in paths)
            {
                foreach (string extension in extensions)
                {
                    foreach (string fileName in fileNames)
                        concatFileNames.Append($",{fileName}.{extension}");
                }

                string command = $"(Get-ChildItem -Path {FormatPath(path)} -Recurse -ErrorAction SilentlyContinue -Include {concatFileNames.ToString().Substring(1)}).fullname";
                string result = SSH.RunCommand(command, null, false, null);
                results.AddRange(result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            }

            return results;
        }

        private string[] GetAvailablePaths()
        {
            string command = @"Get-WmiObject Win32_Logicaldisk -Filter ""DriveType = '3'"" | % {$_.DeviceId}";
            string result = SSH.RunCommand(command, null, false, null);
            return result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void SplitStorePathFile(string pathFileName)
        {
            try
            {
                string workingPathFileName = pathFileName.Replace(@"\", @"/");
                int separatorIndex = workingPathFileName.LastIndexOf(@"/");
                StoreFileName = pathFileName.Substring(separatorIndex + 1);
                StorePath = pathFileName.Substring(0, separatorIndex + 1);
            }
            catch (Exception ex)
            {
                throw new PKCS12Exception($"Error attempting to parse certficate store path={StorePath}, file name={StoreFileName}.", ex);
            }
        }

        private int GetChainLength(string certificates)
        {
            int count = 0;
            int i = 0;
            while ((i = certificates.IndexOf(BEG_DELIM, i)) != -1)
            {
                i += BEG_DELIM.Length;
                count++;
            }
            return count;
        }

        private string FormatStorePassword()
        {
            return (!string.IsNullOrEmpty(StorePassword) ? $"-storepass '{StorePassword}'" : string.Empty);
        }

        private string FormatPath(string path)
        {
            return path + (path.Substring(path.Length - 1) == @"\" ? string.Empty : @"\");
        }
    }
}