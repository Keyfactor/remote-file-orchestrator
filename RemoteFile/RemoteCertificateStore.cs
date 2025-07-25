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

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using Keyfactor.Logging;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto;
using static Keyfactor.Extensions.Orchestrator.RemoteFile.ReenrollmentBase;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    internal class RemoteCertificateStore
    {
        private const string NO_EXTENSION = "noext";
        private const string FULL_SCAN = "fullscan";
        private const string LOCAL_MACHINE_SUFFIX = "|localmachine";

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
        internal IRemoteHandler RemoteHandler { get; set; }
        internal ServerTypeEnum ServerType { get; set; }
        internal List<string> DiscoveredStores { get; set; }
        internal string UploadFilePath { get; set; }
        internal ApplicationSettings.FileTransferProtocolEnum FileTransferProtocol { get; set; }
        internal bool IncludePortInSPN { get; set; }
        internal int SSHPort { get; set; }
        
        private Pkcs12Store CertificateStore;
        private ILogger logger;


        internal RemoteCertificateStore() { }

        internal RemoteCertificateStore(string server, string serverId, string serverPassword, string storeFileAndPath, string storePassword, ApplicationSettings.FileTransferProtocolEnum fileTransferProtocol, int sshPort, bool includePortInSPN)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            logger.MethodEntry(LogLevel.Debug);

            Server = server;
            
            PathFile fullPath = SplitStorePathFile(storeFileAndPath);
            StorePath = fullPath.Path;
            StoreFileName = fullPath.File; 

            ServerId = serverId;
            ServerPassword = serverPassword ?? string.Empty;
            StorePassword = storePassword;
            ServerType = StorePath.Substring(0, 1) == "/" ? ServerTypeEnum.Linux : ServerTypeEnum.Windows;
            UploadFilePath = !string.IsNullOrEmpty(ApplicationSettings.SeparateUploadFilePath) && ServerType == ServerTypeEnum.Linux ? ApplicationSettings.SeparateUploadFilePath : StorePath;
            FileTransferProtocol = fileTransferProtocol;
            SSHPort = sshPort;
            IncludePortInSPN = includePortInSPN;
            logger.LogDebug($"UploadFilePath: {UploadFilePath}");

            if (!IsValueSafeRegex(StorePath + StoreFileName))
            {
                logger.LogDebug("Store path not valid");
                string partialMessage = ServerType == ServerTypeEnum.Windows ? @"'\', ':', " : string.Empty;
                throw new RemoteFileException($"PKCS12 store path {storeFileAndPath} is invalid.  Only alphanumeric, '.', '/', {partialMessage}'-', and '_' characters are allowed in the store path.");
            }
            logger.LogDebug("Store path valid");

            logger.MethodExit(LogLevel.Debug);
        }

        internal RemoteCertificateStore(string server, string serverId, string serverPassword, ServerTypeEnum serverType, int sshPort)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            logger.MethodEntry(LogLevel.Debug);

            Server = server;
            ServerId = serverId;
            ServerPassword = serverPassword ?? string.Empty;
            ServerType = serverType;
            SSHPort = sshPort;

            logger.MethodExit(LogLevel.Debug);
        }

        internal void LoadCertificateStore(ICertificateStoreSerializer certificateStoreSerializer, bool isInventory)
        {
            logger.MethodEntry(LogLevel.Debug);

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            CertificateStore = storeBuilder.Build();

            byte[] byteContents = RemoteHandler.DownloadCertificateFile(StorePath + StoreFileName);
            if (byteContents.Length < 5)
                return;

            CertificateStore = certificateStoreSerializer.DeserializeRemoteCertificateStore(byteContents, StorePath, StorePassword, RemoteHandler, isInventory);

            logger.MethodExit(LogLevel.Debug);
        }

        internal Pkcs12Store GetCertificateStore()
        {
            logger.MethodEntry(LogLevel.Debug);
            logger.MethodExit(LogLevel.Debug);

            return CertificateStore;
        }

        internal void Terminate()
        {
            logger.MethodEntry(LogLevel.Debug);

            if (RemoteHandler != null)
                RemoteHandler.Terminate();

            logger.MethodExit(LogLevel.Debug);
        }

        internal List<string> FindStores(string[] paths, string[] extensions, string[] files, string[] ignoredDirs, bool includeSymLinks)
        {
            logger.MethodEntry(LogLevel.Debug);

            if (!AreValuesSafeRegex(paths))
                throw new RemoteFileException(@"Invalid/unsafe directories to search value supplied.  Only alphanumeric characters are allowed along with special characters of @ and / (Linux) and : and \ (Windows).");
            if (!AreValuesSafeRegex(extensions))
                throw new RemoteFileException(@"Invalid/unsafe file extension value supplied.  Only alphanumeric characters are allowed along with special characters of @ and / (Linux) and : and \ (Windows).");
            if (!AreValuesSafeRegex(files))
                throw new RemoteFileException(@"Invalid/unsafe file name value supplied.  Only alphanumeric characters are allowed along with special characters of @ and / (Linux) and : and \ (Windows).");

            logger.MethodExit(LogLevel.Debug);

            if (DiscoveredStores != null)
                return DiscoveredStores;

            return ServerType == ServerTypeEnum.Linux ? FindStoresLinux(paths, extensions, files, ignoredDirs, includeSymLinks) : FindStoresWindows(paths, extensions, files);
        }

        internal List<X509Certificate2Collection> GetCertificateChains()
        {
            logger.MethodEntry(LogLevel.Debug);

            List<X509Certificate2Collection> certificateChains = new List<X509Certificate2Collection>();

            foreach(string alias in CertificateStore.Aliases)
            {
                X509Certificate2Collection chain = new X509Certificate2Collection();
                X509CertificateEntry[] entries;

                if (CertificateStore.IsKeyEntry(alias))
                {
                    entries = CertificateStore.GetCertificateChain(alias);
                }
                else
                {
                    X509CertificateEntry entry = CertificateStore.GetCertificate(alias);
                    entries = new X509CertificateEntry[] { entry };
                }

                foreach(X509CertificateEntry entry in entries)
                {
                    X509Certificate2Ext cert = new X509Certificate2Ext(entry.Certificate.GetEncoded());
                    cert.FriendlyNameExt = alias;
                    cert.HasPrivateKey = CertificateStore.IsKeyEntry(alias);
                    chain.Add(cert);
                }

                certificateChains.Add(chain);
            }

            logger.MethodExit(LogLevel.Debug);

            return certificateChains;
        }

        internal void DeleteCertificateByAlias(string alias)
        {
            logger.MethodEntry(LogLevel.Debug);

            try
            {
                byte[] byteContents = RemoteHandler.DownloadCertificateFile(StorePath + StoreFileName);

                using (MemoryStream stream = new MemoryStream(byteContents))
                {
                    if (stream.Length == 0)
                    {
                        throw new RemoteFileException($"Alias {alias} does not exist in certificate store {StorePath + StoreFileName}.");
                    }

                    if (!CertificateStore.ContainsAlias(alias))
                    {
                        throw new RemoteFileException($"Alias {alias} does not exist in certificate store {StorePath + StoreFileName}.");
                    }

                    CertificateStore.DeleteEntry(alias);

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        CertificateStore.Save(outStream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to remove certficate for store path={StorePath}, file name={StoreFileName}.", ex);
            }

            logger.MethodExit(LogLevel.Debug);
        }

        internal void CreateCertificateStore(ICertificateStoreSerializer certificateStoreSerializer, string storePath, string linuxFilePermissions, string linuxFileOwner)
        {
            logger.MethodEntry(LogLevel.Debug);

            RemoteHandler.CreateEmptyStoreFile(storePath, linuxFilePermissions, linuxFileOwner);
            string privateKeyPath = certificateStoreSerializer.GetPrivateKeyPath();
            if (!string.IsNullOrEmpty(privateKeyPath))
                RemoteHandler.CreateEmptyStoreFile(privateKeyPath, linuxFilePermissions, linuxFileOwner);


            logger.MethodExit(LogLevel.Debug);
        }

        internal void AddCertificate(string alias, string certificateEntry, bool overwrite, string pfxPassword, bool removeRootCertificate)
        {
            logger.MethodEntry(LogLevel.Debug);
            logger.LogDebug($"Alias: {alias}, Certificate Entry: {certificateEntry}, Overwrite: {overwrite}, RemoveRootCertificate: {removeRootCertificate}");

            try
            {
                Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
                Pkcs12Store certs = storeBuilder.Build();

                byte[] newCertBytes = removeRootCertificate && !string.IsNullOrEmpty(pfxPassword) ? 
                    RemoveRootCertificate(Convert.FromBase64String(certificateEntry), pfxPassword) : 
                    Convert.FromBase64String(certificateEntry);

                Pkcs12Store newEntry = storeBuilder.Build();

                X509Certificate2 cert = new X509Certificate2(newCertBytes, pfxPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
                byte[] binaryCert = cert.Export(X509ContentType.Pkcs12, pfxPassword);

                using (MemoryStream ms = new MemoryStream(string.IsNullOrEmpty(pfxPassword) ? binaryCert : newCertBytes))
                {
                    newEntry.Load(ms, string.IsNullOrEmpty(pfxPassword) ? new char[0] : pfxPassword.ToCharArray());
                }

                if (CertificateStore.ContainsAlias(alias) && !overwrite)
                {
                    throw new RemoteFileException($"Alias {alias} already exists in store {StorePath + StoreFileName} and overwrite is set to False.  Please try again with overwrite set to True if you wish to replace this entry.");
                }

                string checkAliasExists = string.Empty;
                foreach (string newEntryAlias in newEntry.Aliases)
                {
                    if (!newEntry.IsKeyEntry(newEntryAlias))
                        continue;

                    checkAliasExists = newEntryAlias;

                    if (CertificateStore.ContainsAlias(alias))
                    {
                        CertificateStore.DeleteEntry(alias);
                    }
                    CertificateStore.SetKeyEntry(alias, newEntry.GetKey(newEntryAlias), newEntry.GetCertificateChain(newEntryAlias));
                }

                if (string.IsNullOrEmpty(checkAliasExists))
                {
                    Org.BouncyCastle.X509.X509Certificate bcCert = DotNetUtilities.FromX509Certificate(cert);
                    X509CertificateEntry bcEntry = new X509CertificateEntry(bcCert);
                    if (CertificateStore.ContainsAlias(alias))
                    {
                        CertificateStore.DeleteEntry(alias);
                    }
                    CertificateStore.SetCertificateEntry(alias, bcEntry);
                }

                using (MemoryStream outStream = new MemoryStream())
                {
                    CertificateStore.Save(outStream, string.IsNullOrEmpty(StorePassword) ? new char[0] : StorePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                }
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to add certficate for store path={StorePath}, file name={StoreFileName}.", ex);
            }

            logger.MethodExit(LogLevel.Debug);
        }

        internal void SaveCertificateStore(List<SerializedStoreInfo> storeInfo)
        {
            logger.MethodEntry(LogLevel.Debug);

            foreach(SerializedStoreInfo fileInfo in storeInfo)
            {
                PathFile pathFile = SplitStorePathFile(fileInfo.FilePath);
                RemoteHandler.UploadCertificateFile(pathFile.Path, pathFile.File, fileInfo.Contents);
            }

            logger.MethodExit(LogLevel.Debug);
        }

        internal bool DoesStoreExist()
        {
            logger.MethodEntry(LogLevel.Debug);
            logger.MethodExit(LogLevel.Debug);

            return RemoteHandler.DoesFileExist(StorePath + StoreFileName);
        }

        internal static PathFile SplitStorePathFile(string pathFileName)
        {
            try
            {
                string storePathFileName = pathFileName.Replace(@"\", @"/");
                int separatorIndex = storePathFileName.LastIndexOf(@"/");

                return new PathFile() { Path = pathFileName.Substring(0, separatorIndex + 1), File = pathFileName.Substring(separatorIndex + 1) };
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to parse certficate store path={pathFileName}.", ex);
            }
        }

        internal string GenerateCSR(string subjectText, SupportedKeyTypeEnum keyType, int keySize, List<string> sans)
        {
            IAsymmetricCipherKeyPairGenerator keyPairGenerator = null;
            string algorithm = string.Empty;
            switch (keyType)
            {
                case SupportedKeyTypeEnum.RSA:
                    keyPairGenerator = new RsaKeyPairGenerator();
                    algorithm = "SHA256withRSA";
                    break;
                case SupportedKeyTypeEnum.ECC:
                    keyPairGenerator = new ECKeyPairGenerator();
                    algorithm = "SHA256withECDSA";
                    if (keySize == 384) algorithm = "SHA384withECDSA";
                    if (keySize == 521) algorithm = "SHA512withECDSA";
                    break;
            }

            var keyGenParams = new KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(new CryptoApiRandomGenerator()), keySize);
            keyPairGenerator.Init(keyGenParams);
            var keyPair = keyPairGenerator.GenerateKeyPair();

            var subject = new X509Name(subjectText);

            // Add SAN entries
            var subAltNameList = new List<GeneralName>();
            sans.ForEach(san => subAltNameList.Add(new GeneralName(GeneralName.DnsName, san.Trim())));
            var generalSubAltNames = new GeneralNames(subAltNameList.ToArray());

            var extensionsGenerator = new X509ExtensionsGenerator();
            extensionsGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, generalSubAltNames);
            var attributeSet = new DerSet(new AttributePkcs(PkcsObjectIdentifiers.Pkcs9AtExtensionRequest, new DerSet(extensionsGenerator.Generate())));

            Pkcs10CertificationRequest csr = new Pkcs10CertificationRequest(algorithm, subject, keyPair.Public, (DerSet)attributeSet, keyPair.Private);

            // encode the CSR as base64
            var encodedCsr = Convert.ToBase64String(csr.GetEncoded());
            return encodedCsr;
        }

        internal string GenerateCSROnDevice(string subjectText, SupportedKeyTypeEnum keyType, int keySize, List<string> sans, out string privateKey)
        {
            string path = ApplicationSettings.TempFilePathForODKG;
            if (path.Substring(path.Length - 1, 1) != "/") path += "/";
            string fileName = Guid.NewGuid().ToString();

            X500DistinguishedName dn = new X500DistinguishedName(subjectText);
            string opensslSubject = dn.Format(true).Replace("S=","ST=");
            opensslSubject = opensslSubject.Replace(System.Environment.NewLine, "/");
            opensslSubject = "/" + opensslSubject.Substring(0, opensslSubject.Length - 1);

            string cmd = $"openssl req -new -newkey REPLACE -nodes -keyout {path}{fileName}.key -out {path}{fileName}.csr -subj '{opensslSubject}'";
            switch (keyType)
            {
                case SupportedKeyTypeEnum.RSA:
                    cmd = cmd.Replace("REPLACE", $"rsa:{keySize.ToString()}");
                    break;
                case SupportedKeyTypeEnum.ECC:
                    string algName = "prime256v1";
                    switch (keySize)
                    {
                        case 384:
                            algName = "secp384r1";
                            break;
                        case 521:
                            algName = "secp521r1";
                            break;
                    }
                    cmd = cmd.Replace("REPLACE", $"ec:<(openssl ecparam -name {algName})");
                    break;
            }

            string csr = string.Empty;
            privateKey = string.Empty;
            try
            {
                try
                {
                    RemoteHandler.RunCommand(cmd, null, ApplicationSettings.UseSudo, null);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("----"))
                        throw;
                }

                privateKey = Encoding.UTF8.GetString(RemoteHandler.DownloadCertificateFile(path + fileName + ".key"));
                csr = Encoding.UTF8.GetString(RemoteHandler.DownloadCertificateFile(path + fileName + ".csr"));
            }
            finally
            {
                if (RemoteHandler.DoesFileExist(path + fileName + ".key"))
                    RemoteHandler.RemoveCertificateFile(path, fileName + ".key");
                if (RemoteHandler.DoesFileExist(path + fileName + ".csr"))
                    RemoteHandler.RemoveCertificateFile(path, fileName + ".csr");
            }

            return csr;
        }

        internal void Initialize(string sudoImpersonatedUser)
        {
            logger.MethodEntry(LogLevel.Debug);

            bool treatAsLocal = Server.ToLower().EndsWith(LOCAL_MACHINE_SUFFIX);

            if (ServerType == ServerTypeEnum.Linux || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                RemoteHandler = treatAsLocal ? new LinuxLocalHandler() as IRemoteHandler : new SSHHandler(Server, ServerId, ServerPassword, ServerType == ServerTypeEnum.Linux, FileTransferProtocol, SSHPort, sudoImpersonatedUser) as IRemoteHandler;
            else
                RemoteHandler = new WinRMHandler(Server, ServerId, ServerPassword, treatAsLocal, IncludePortInSPN);

            logger.MethodExit(LogLevel.Debug);
        }

        private byte[] RemoveRootCertificate(byte[] binCert, string password)
        {
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();
            Pkcs12Store store2 = storeBuilder.Build();

            byte[] rtnCert = new byte[1];

            using (MemoryStream ms = new MemoryStream(binCert))
            {
                store.Load(ms, password.ToCharArray());
            }

            foreach (string alias in store.Aliases)
            {
                X509CertificateEntry[] chain = store.GetCertificateChain(alias);
                chain = chain.Where(p => p.Certificate.SubjectDN.ToString() != p.Certificate.IssuerDN.ToString()).ToArray();
                store2.SetKeyEntry(alias, store.GetKey(alias), chain);
                
                using (MemoryStream ms = new MemoryStream())
                {
                    store2.Save(ms, password.ToCharArray(), new SecureRandom());
                    rtnCert = ms.ToArray();
                }
            }

            return rtnCert;
        }

        private bool AreValuesSafeRegex(string[] values)
        {
            bool valueIsSafe = true;
            foreach(string value in values)
            {
                valueIsSafe = IsValueSafeRegex(value.Replace("*",String.Empty));
                if (!valueIsSafe)
                    break;
            }
            return valueIsSafe;
        }

        private bool IsValueSafeRegex(string value)
        {
            logger.MethodEntry(LogLevel.Debug);

            Regex regex = new Regex(ServerType == ServerTypeEnum.Linux ? $@"^[\d\s\w-_/@.]*$" : $@"^[\d\s\w-_/.:)(\\\\]*$");

            logger.MethodExit(LogLevel.Debug);

            return regex.IsMatch(value);
        }

        private List<string> FindStoresLinux(string[] paths, string[] extensions, string[] fileNames, string[] ignoredDirs, bool includeSymLinks)
        {
            logger.MethodEntry(LogLevel.Debug);
            
            try
            {
                string concatPaths = string.Join(" ", paths);
                string command = $"find {concatPaths} -path /proc -prune -o ";

                foreach (string ignoredDir in ignoredDirs)
                {
                    command += $"-path {ignoredDir} -prune -o ";
                }

                if (!includeSymLinks)
                    command += " -type f ";

                command += "\\( ";
                foreach (string extension in extensions)
                {
                    foreach (string fileName in fileNames)
                    {
                        command += (command.IndexOf("-name") == -1 ? string.Empty : "-or ");
                        command += $"-name '{fileName.Trim()}";
                        if (extension.ToLower() == NO_EXTENSION)
                            command += $"' ! -name '*.*' ";
                        else
                            command += $".{extension.Trim()}' ";
                    }
                }

                command += " \\) -print";

                string result = string.Empty;
                //if (extensions.Any(p => p.ToLower() != NO_EXTENSION))
                    result = RemoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, null);

                logger.MethodExit(LogLevel.Debug);

                return (result.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            }
            catch (Exception ex)
            {
                throw new RemoteFileException($"Error attempting to find certificate stores for path={string.Join(" ", paths)}.", ex);
            }
        }

        private List<string> FindStoresWindows(string[] paths, string[] extensions, string[] fileNames)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<string> results = new List<string>();
            bool hasNoExt = false;

            if (paths[0].ToLower() == FULL_SCAN)
            {
                paths = GetAvailableDrives();
                for (int i = 0; i < paths.Length; i++)
                    paths[i] += "/";
            }

            foreach (string path in paths)
            {
                StringBuilder concatFileNames = new StringBuilder();

                foreach (string extension in extensions)
                {
                    if (extension.ToLower() == NO_EXTENSION)
                    {
                        hasNoExt = true;
                    }
                    else
                    {
                        foreach (string fileName in fileNames)
                            concatFileNames.Append($",{fileName}.{extension}");
                    }
                }

                if (concatFileNames.Length > 0)
                {
                    string command = $"(Get-ChildItem -Path {FormatPath(path)} -File -Recurse -ErrorAction SilentlyContinue -Include {concatFileNames.ToString().Substring(1)}).fullname";
                    string result = RemoteHandler.RunCommand(command, null, false, null);
                    results.AddRange(result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
                }

                if (hasNoExt)
                {
                    concatFileNames = new StringBuilder();

                    foreach (string fileName in fileNames)
                        concatFileNames.Append($",{fileName}");

                    if (concatFileNames.Length > 0)
                    {
                        string command = $"(Get-ChildItem -Path {FormatPath(path)} -File -Recurse -ErrorAction SilentlyContinue -Include {concatFileNames.ToString().Substring(1)} -Filter *.).fullname";
                        string result = RemoteHandler.RunCommand(command, null, false, null);
                        results.AddRange(result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
                    }
                }

                logger.MethodExit(LogLevel.Debug);
            }

            return results;
        }

        private string[] GetAvailableDrives()
        {
            logger.MethodEntry(LogLevel.Debug);

            string command = @"Get-WmiObject Win32_Logicaldisk -Filter ""DriveType = '3'"" | % {$_.DeviceId}";
            string result = RemoteHandler.RunCommand(command, null, false, null);

            logger.MethodExit(LogLevel.Debug);

            return result.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string FormatPath(string path)
        {
            logger.MethodEntry(LogLevel.Debug);
            logger.MethodExit(LogLevel.Debug);

            return "'" + path + (path.Substring(path.Length - 1) == @"\" ? string.Empty : @"\") + "'";
        }
    }

    class PathFile
    {
        public string Path { get; set; }
        public string File { get; set; }
    }
}