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
using Keyfactor.Extensions.Orchestrator.RemoteFile.PKCS12;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.OraWlt
{
    class OraWltCertificateStoreSerializer : ICertificateStoreSerializer
    {
        private ILogger logger;

        public OraWltCertificateStoreSerializer(string storeProperties) 
        {
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContentBytes, string storePath, string storePassword, IRemoteHandler remoteHandler, bool isInventory)
        {
            logger.MethodEntry(LogLevel.Debug);

            PKCS12CertificateStoreSerializer serializer = new PKCS12CertificateStoreSerializer(string.Empty);

            //string tempStoreFile = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".p12";
            //string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            //string orapkiCommand = $"orapki wallet pkcs12_to_jks -wallet \"{WorkFolder}{tempStoreFile}\" -pwd \"{storePassword}\" -jksKeyStoreLoc \"{WorkFolder}{tempStoreFileJKS}\" -jksKeyStorepwd \"{storePassword}\"";

            //JksStore jksStore = new JksStore();
            //Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
            //Pkcs12Store store = storeBuilder.Build();

            //try
            //{
            //    remoteHandler.UploadCertificateFile(WorkFolder, tempStoreFile, storeContentBytes);

            //    remoteHandler.RunCommand(orapkiCommand, null, ApplicationSettings.UseSudo, null);

            //    byte[] storeBytes = remoteHandler.DownloadCertificateFile($"{WorkFolder}{tempStoreFileJKS}");
            //    jksStore.Load(new MemoryStream(storeBytes), string.IsNullOrEmpty(storePassword) ? new char[0] : storePassword.ToCharArray());

            //    JKSCertificateStoreSerializer serializer = new JKSCertificateStoreSerializer(String.Empty);
            //    store = serializer.DeserializeRemoteCertificateStore(storeBytes, $"{WorkFolder}{tempStoreFileJKS}", storePassword, remoteHandler, isInventory);
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
            //finally
            //{
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFile); } catch (Exception) { };
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFile+".lck"); } catch (Exception) { };
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFileJKS); } catch (Exception) { };
            //}

            logger.MethodExit(LogLevel.Debug);

            return serializer.DeserializeRemoteCertificateStore(storeContentBytes, storePath, storePassword, remoteHandler, isInventory);
            //return store;
        }

        public List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler)
        {
            logger.MethodEntry(LogLevel.Debug);

            PKCS12CertificateStoreSerializer serializer = new PKCS12CertificateStoreSerializer(string.Empty);

            try
            {
                return serializer.SerializeRemoteCertificateStore(certificateStore, storePath, storeFileName, storePassword, remoteHandler);
            }
            finally
            {
                logger.MethodExit(LogLevel.Debug);
            }

            //List<SerializedStoreInfo> storeInfo = new List<SerializedStoreInfo>();

            //string tempStoreFileJKS = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".jks";

            //string orapkiCommand1 = $"orapki wallet create -wallet \"{WorkFolder}\" -pwd \"{storePassword}\"";
            //string orapkiCommand2 = $"orapki wallet jks_to_pkcs12 -wallet \"{WorkFolder}\" -pwd \"{storePassword}\" -keystore \"{WorkFolder}{tempStoreFileJKS}\" -jkspwd \"{storePassword}\"";

            //JksStore jksStore = new JksStore();

            //JKSCertificateStoreSerializer serializer = new JKSCertificateStoreSerializer(string.Empty);
            //List<SerializedStoreInfo> jksStoreInfo = serializer.SerializeRemoteCertificateStore(certificateStore, WorkFolder, storeFileName, storePassword, remoteHandler);

            //try
            //{
            //    remoteHandler.UploadCertificateFile($"{WorkFolder}", $"{tempStoreFileJKS}", jksStoreInfo[0].Contents);
            //    remoteHandler.RunCommand(orapkiCommand1, null, ApplicationSettings.UseSudo, [storePassword]);
            //    remoteHandler.RunCommand(orapkiCommand2, null, ApplicationSettings.UseSudo, [storePassword]);

            //    byte[] storeContents = remoteHandler.DownloadCertificateFile($"{WorkFolder}ewallet.p12");

            //    storeInfo.Add(new SerializedStoreInfo() { Contents = storeContents, FilePath = storePath+storeFileName });
            //    return storeInfo;
            //}
            //finally
            //{
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, "ewallet.p12"); } catch (Exception) { }
            //    ;
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, "ewallet.p12.lck"); } catch (Exception) { }
            //    ;
            //    try { remoteHandler.RemoveCertificateFile(WorkFolder, tempStoreFileJKS); } catch (Exception) { }
            //    ;
            //}
        }

        public string GetPrivateKeyPath()
        {
            return null;
        }
    }
}
