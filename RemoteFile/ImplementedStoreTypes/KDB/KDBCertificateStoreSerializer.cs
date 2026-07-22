// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Pkcs;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    class KDBCertificateStoreSerializer : ICertificateStoreSerializer, ICustomFileCreator
    {
        private ILogger logger;

        public KDBCertificateStoreSerializer(string storeProperties)
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            logger.MethodEntry(LogLevel.Debug);

            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;
            if (storePath.Substring(0, 1) == "|")
                storePath = "/" + storePath.Substring(1);

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".kdb";
            string tempCertFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";

            remoteHandler.UploadCertificateFile(storePath, tempStoreFile, storeContentBytes);
            
            string command = $"{bashCommand}gskcapicmd -keydb -convert -db \"{storePath}{tempStoreFile}\" -pw \"{storePassword}\" -new_db \"{storePath}{tempCertFile}\" -new_pw \"{storePassword}\" -new_format p12";

            try
            {
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, new string[] { storePassword });

                byte[] storeBytes = remoteHandler.DownloadCertificateFile($"{storePath}{tempCertFile}");
                store.Load(new MemoryStream(storeBytes), string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("cannot execute binary file", StringComparison.InvariantCultureIgnoreCase) && storePath.Substring(0, 1) == "/")
                {
                    storePath = "|" + storePath.Substring(1);
                    store = DeserializeRemoteCertificateStore(storeContentBytes, storePath, storePassword, remoteHandler, isInventory);
                }
                else
                    throw;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(storePath, tempStoreFile); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(storePath, tempCertFile); } catch (Exception) { };
            }

            logger.MethodExit(LogLevel.Debug);
            return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;
            if (storePath.Substring(0, 1) == "|")
                storePath = "/" + storePath.Substring(1);

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".kdb";
            string tempCertFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";

            string command = $"{bashCommand}gskcapicmd -keydb -convert -db \"{storePath}{tempCertFile}\" -pw \"{storePassword}\" -type p12 -new_db \"{storePath}{tempStoreFile}\" -new_pw \"{storePassword}\" -new_format cms";
            
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    certificateStore.Save(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                    remoteHandler.UploadCertificateFile(storePath, tempCertFile, ms.ToArray());
                }
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, new string[] { storePassword });
                byte[] storeContents = remoteHandler.DownloadCertificateFile($"{storePath}{tempStoreFile}");

                storeInfo.Add(new SerializedStoreInfo() { Contents = storeContents, FilePath = storePath+storeFileName });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("cannot execute binary file", StringComparison.InvariantCultureIgnoreCase) && storePath.Substring(0, 1) == "/")
                {
                    storePath = "|" + storePath.Substring(1);
                    storeInfo = SerializeRemoteCertificateStore(certificateStore, storePath, storeFileName, storePassword, remoteHandler);
                }
                else
                    throw;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(storePath, tempStoreFile); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(storePath, tempCertFile); } catch (Exception) { };
            }

            return storeInfo;
        }

        public string GetPrivateKeyPath() 
        {
            return null;
        }

        public void CreateEmptyStoreFile(string storePath, string storePassword, string linuxFilePermissions, string linuxFileOwner, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            int extIdx = storePath.LastIndexOf('.');
            if (extIdx == -1)
                throw new Exception("Store path must include a file name with an extension.");
            
            string pathDelim = storePath.Substring(0, 1) == "/" ? "/" : "\\";
            int fileNameIdx = storePath.LastIndexOf(pathDelim) + 1;
            if (fileNameIdx == -1)
                throw new Exception("Invalid format for store path.");

            string path = storePath.Substring(0, fileNameIdx);
            string fileName = storePath.Substring(fileNameIdx, extIdx - fileNameIdx);

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty);
            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;

            string command = $"{bashCommand}gskcapicmd -keydb -create -db \"{path}{tempStoreFile + ".kdb"}\" -pw \"{storePassword}\" -type cms -stash";

            try
            {
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, new string[] { storePassword });

                remoteHandler.CreateEmptyStoreFile(path + fileName + ".kdb", linuxFilePermissions, linuxFileOwner);
                remoteHandler.CreateEmptyStoreFile(path + fileName + ".rdb", linuxFilePermissions, linuxFileOwner);
                remoteHandler.CreateEmptyStoreFile(path + fileName + ".crl", linuxFilePermissions, linuxFileOwner);
                remoteHandler.CreateEmptyStoreFile(path + fileName + ".sth", linuxFilePermissions, linuxFileOwner);

                remoteHandler.RunCommand($"cp {path}{tempStoreFile}.kdb {path}{fileName}.kdb", null, ApplicationSettings.UseSudo, null);
                remoteHandler.RunCommand($"cp {path}{tempStoreFile}.rdb {path}{fileName}.rdb", null, ApplicationSettings.UseSudo, null);
                remoteHandler.RunCommand($"cp {path}{tempStoreFile}.crl {path}{fileName}.crl", null, ApplicationSettings.UseSudo, null);
                remoteHandler.RunCommand($"cp {path}{tempStoreFile}.sth {path}{fileName}.sth", null, ApplicationSettings.UseSudo, null);
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(path, tempStoreFile + ".kdb"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(path, tempStoreFile + ".sth"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(path, tempStoreFile + ".rdb"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(path, tempStoreFile + ".crl"); } catch (Exception) { };
            }


            logger.MethodExit(LogLevel.Debug);
        }
    }
}
