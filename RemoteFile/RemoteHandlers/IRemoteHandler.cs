// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.Orchestrator.RemoteFile.RemoteHandlers
{
    /// <summary>
    /// Defines the interface that must be implemented by the method used to send data across the wire (i.e. SSH or WinRM via PS)
    /// Currently with the dependency on the SSH class, need to look into refactoring to the inerface to allow SSH or WimRM
    /// </summary>
    interface IRemoteHandler
    {
        void Terminate();

        string RunCommand(string commandText, object[] arguments, bool withSudo, string[] passwordsToMaskInLog);

        void UploadCertificateFile(string path, string fileName, byte[] certBytes);

        byte[] DownloadCertificateFile(string path);

        void CreateEmptyStoreFile(string path, string linuxFilePermissions, string linuxFileOwner);

        bool DoesFileExist(string path);

        void RemoveCertificateFile(string path, string fileName);
    }
}
