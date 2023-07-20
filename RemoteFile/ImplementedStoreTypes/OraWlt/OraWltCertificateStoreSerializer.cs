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
using Keyfactor.Extensions.Orchestrator.RemoteFile.JKS;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.OraWlt
{
    class OraWltCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;

        public OraWltCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;
            
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();
            JksStore jksStore = new JksStore();

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";
            string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            remoteHandler.UploadCertificateFile(storePath, tempStoreFile, storeContentBytes);
            
            string command = $"orapki wallet pkcs12_to_jks -wallet \"{storePath}{tempStoreFile}\" -pwd \"{storePassword}\" -jksKeyStoreLoc \"{storePath}{tempStoreFileJKS}\" -jksKeyStorepwd \"{storePassword}\"";
            
            try
            {
                remoteHandler.RunCommand(command, null, ApplicationSettings.UseSudo, null);

                byte[] storeBytes = remoteHandler.DownloadCertificateFile($"{storePath}{tempStoreFileJKS}");
                jksStore.Load(new MemoryStream(storeBytes), string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());
                JKSCertificateStoreSerializer.ConvertToPkcs12Store(jksStore, store, storePassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(storePath, tempStoreFile); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(storePath, tempStoreFileJKS); } catch (Exception) { };
            }

            logger.MethodExit(LogLevel.Debug);
            return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();
            string bashCommand = storePath.Substring(0, 1) == "/" ? "bash " : string.Empty;

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";
            string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            JksStore jksStore = new JksStore();

            JKSCertificateStoreSerializer.ConvertToJKSStore(certificateStore, jksStore, storePassword);

            string command = $"orapki wallet jks_to_pkcs12 -wallet \"{storePath}{tempStoreFile}\" -pwd \"{storePassword}\" -keystore \"{storePath}{tempStoreFileJKS}\" -jkspwd \"{storePassword}\"";

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

        public string GetPrivateKeyPath()
        {
            return null;
        }
    }
}
