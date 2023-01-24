// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Org.BouncyCastle.Pkcs;
using Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers;
using Keyfactor.Extensions.Orchestrator.RemoteFile.Models;
using System.Collections.Generic;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    interface ICertificateStoreSerializer
    {
        Pkcs12Store DeserializeRemoteCertificateStore(byte[] storeContents, string storePath, string storePassword, IRemoteHandler remoteHandler);

        List<SerializedStoreInfo> SerializeRemoteCertificateStore(Pkcs12Store certificateStore, string storePath, string storeFileName, string storePassword, IRemoteHandler remoteHandler);

        string GetPrivateKeyPath();
    }
}
