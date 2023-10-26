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
using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.OraWlt
{
    class OraWltCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;

        public string WorkFolder { get; set; }

        public OraWltCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
            LoadCustomProperties(storeProperties);
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool includePrivateKey)
        {
            logger.MethodEntry(LogLevel.Debug);

            string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";
            string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            string orapkiCommand = $"orapki wallet pkcs12_to_jks -wallet \"{WorkFolder}{tempStoreFile}\" -pwd \"{storePassword}\" -jksKeyStoreLoc \"{WorkFolder}{tempStoreFileJKS}\" -jksKeyStorepwd \"{storePassword}\"";

            JksStore jksStore = new JksStore();
            Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            Pkcs12Store store = storeBuilder.Build();

            try
            {
                remoteHandler.UploadCertificateFile(WorkFolder, tempStoreFile, storeContentBytes);

                remoteHandler.RunCommand(orapkiCommand, null, ApplicationSettings.UseSudo, null);

                byte[] storeBytes = remoteHandler.DownloadCertificateFile($"{WorkFolder}{tempStoreFileJKS}");
                jksStore.Load(new MemoryStream(storeBytes), string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());

                JKSCertificateStoreSerializer serializer = new JKSCertificateStoreSerializer(String.Empty);
                store = serializer.DeserializeRemoteCertificateStore(storeBytes, $"{WorkFolder}{tempStoreFileJKS}", storePassword, remoteHandler, includePrivateKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFile); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFile+".lck"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFileJKS); } catch (Exception) { };
            }

            logger.MethodExit(LogLevel.Debug);
            return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            string orapkiCommand1 = $"orapki wallet create -wallet \"{WorkFolder}\" -pwd \"{storePassword}\"";
            string orapkiCommand2 = $"orapki wallet jks_to_pkcs12 -wallet \"{WorkFolder}\" -pwd \"{storePassword}\" -keystore \"{WorkFolder}{tempStoreFileJKS}\" -jkspwd \"{storePassword}\"";

            JksStore jksStore = new JksStore();

            JKSCertificateStoreSerializer serializer = new JKSCertificateStoreSerializer(string.Empty);
            List<SerializedStoreInfo> jksStoreInfo = serializer.SerializeRemoteCertificateStore(certificateStore, WorkFolder, storeFileName, storePassword, remoteHandler);

            try
            {
                remoteHandler.UploadCertificateFile($"{WorkFolder}", $"{tempStoreFileJKS}", jksStoreInfo[0].Contents);
                remoteHandler.RunCommand(orapkiCommand1, null, ApplicationSettings.UseSudo, null);
                remoteHandler.RunCommand(orapkiCommand2, null, ApplicationSettings.UseSudo, null);

                byte[] storeContents = remoteHandler.DownloadCertificateFile($"{WorkFolder}ewallet.p12");

                storeInfo.Add(new SerializedStoreInfo() { Contents = storeContents, FilePath = storePath+storeFileName });
                return storeInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                try { remoteHandler.RemoveCertificateFile(WorkFolder, "ewallet.p12"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(WorkFolder, "ewallet.p12.lck"); } catch (Exception) { };
                try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFileJKS); } catch (Exception) { };
            }
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }

        public bool HasPrivateKeyOverride()
        {
            return false;
        }

        private void LoadCustomProperties(string storeProperties)
        {
            logger.MethodEntry(LogLevel.Debug);

            dynamic properties = JsonConvert.DeserializeObject(storeProperties);
            WorkFolder = properties.WorkFolder == null || string.IsNullOrEmpty(properties.WorkFolder.Value) ? String.Empty : properties.WorkFolder.Value;

            string pathDelimiter = @"\";
            if (WorkFolder.Substring(0, 1) == @"/")
                pathDelimiter = @"/";

            if (WorkFolder.Substring(WorkFolder.Length - 1, 1) != pathDelimiter)
                WorkFolder += pathDelimiter;

            logger.MethodExit(LogLevel.Debug);
        }

    }
}
