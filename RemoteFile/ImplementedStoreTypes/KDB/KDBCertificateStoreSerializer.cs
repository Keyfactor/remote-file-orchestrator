// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;

using Keyfactor.Logging;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Pkcs;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    class KDBCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;

        public KDBCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool includePrivateKey)
        {
            logger.MethodEntry(LogLevel.Debug);

            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;

            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".kdb";
            string tempCertFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";

            remoteHandler.UploadCertificateFile(storePath, tempStoreFile, storeContentBytes);
            
            string command = $"{bashCommand}gskcapicmd -keydb -convert -db \"{storePath}{tempStoreFile}\" -pw \"{storePassword}\" -type kdb -new_db \"{storePath}{tempCertFile}\" -new_pw \"{storePassword}\" -new_format p12";

            try
            {
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, null);

                byte[] storeBytes = remoteHandler.DownloadCertificateFile($"{storePath}{tempCertFile}");
                store.Load(new MemoryStream(storeBytes), string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
            }
            catch (Exception ex)
            {
                throw ex;
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

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".kdb";
            string tempCertFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";

            string command = $"{bashCommand}gskcapicmd -keydb -convert -db \"{storePath}{tempCertFile}\" -pw \"{storePassword}\" -type p12 -new_db \"{storePath}{tempStoreFile}\" -new_pw \"{storePassword}\" -new_format kdb";
            
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    certificateStore.Save(ms, string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray(), new Org.BouncyCastle.Security.SecureRandom());
                    remoteHandler.UploadCertificateFile(storePath, tempCertFile, ms.ToArray());
                }
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, null);
                byte[] storeContents = remoteHandler.DownloadCertificateFile($"{storePath}{tempStoreFile}");

                storeInfo.Add(new SerializedStoreInfo() { Contents = storeContents, FilePath = storePath+storeFileName });
                return storeInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(storePath, tempStoreFile); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(storePath, tempCertFile); } catch (Exception) { };
            }
        }

        public bool HasPrivateKeyOverride()
        {
            return false;
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }
    }
}
