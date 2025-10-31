// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;
using static Keyfactor.PKI.PKIConstants.X509;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile
{
    public abstract class ReenrollmentBase : RemoteFileJobTypeBase, IReenrollmentJobExtension
    {
        public string ExtensionName => "Keyfactor.Extensions.Orchestrator.RemoteFile";

        internal RemoteCertificateStore certificateStore = new RemoteCertificateStore();

        internal enum SupportedKeyTypeEnum
        {
            RSA,
            ECC
        }

        //TODO:
        // 1) Set SANs, Alias and Overwrite "for real" once product figures out how to pass that
        // 2) Add "CreateCSROnDevice" (Y/N) to config.json
        // 3) Add "TempFilePathForODKG" (string) to config.json
        // 4) Add Reenrollment to manifest.json for all store types
        // 5) Rename ProcessJobToDo to ProcessJob
        // 6) Modify ReenrollmentBase to implement IReenrollmentJobExtension 
        // 6) Update README.  Remember to explain the differences between ODKG and OOKG

        public JobResult ProcessJob(ReenrollmentJobConfiguration config, SubmitReenrollmentCSR submitReenrollment)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());

            ICertificateStoreSerializer certificateStoreSerializer = GetCertificateStoreSerializer(config.CertificateStoreDetails.Properties);

            try
            {
                SetJobProperties(config, config.CertificateStoreDetails, logger);

                certificateStore = new RemoteCertificateStore(config.CertificateStoreDetails.ClientMachine, UserName, UserPassword, config.CertificateStoreDetails.StorePath, StorePassword, SSHPort, IncludePortInSPN);
                certificateStore.Initialize(SudoImpersonatedUser, UseShellCommands);

                if (!certificateStore.DoesStoreExist())
                {
                    if (ApplicationSettings.CreateStoreIfMissing)
                        certificateStore.CreateCertificateStore(certificateStoreSerializer, config.CertificateStoreDetails.Properties, config.CertificateStoreDetails.StorePath, logger);
                    else
                        throw new RemoteFileException($"Certificate store {config.CertificateStoreDetails.StorePath} does not exist on server {config.CertificateStoreDetails.ClientMachine}.");
                }

                // validate parameters
                string KeyTypes = string.Join(",", Enum.GetNames(typeof(SupportedKeyTypeEnum)));
                if (!Enum.TryParse(KeyType.ToUpper(), out SupportedKeyTypeEnum KeyTypeEnum))
                {
                    throw new RemoteFileException($"Unsupported KeyType value {KeyType}.  Supported types are {KeyTypes}.");
                }
                
                PathFile storePathFile = RemoteCertificateStore.SplitStorePathFile(config.CertificateStoreDetails.StorePath);

                // generate CSR and call back to enroll certificate
                string csr = string.Empty;
                AsymmetricAlgorithm privateKey;
                if (CreateCSROnDevice)
                {
                    csr = certificateStore.GenerateCSROnDevice(SubjectText, config.Overwrite, config.Alias, KeyTypeEnum, KeySize, config.SANs, out privateKey);
                }
                else
                {
                    csr = certificateStore.GenerateCSR(SubjectText, config.Overwrite, config.Alias, KeyTypeEnum, KeySize, config.SANs, out privateKey);
                }

                X509Certificate2 cert = submitReenrollment.Invoke(csr);

                if (cert == null)
                    throw new RemoteFileException("Enrollment of CSR failed.  Please check Keyfactor Command logs for more information on potential enrollment errors.");

                switch (privateKey)
                {
                    case RSA rsa:
                        cert = cert.CopyWithPrivateKey(rsa);
                        break;

                    case ECDsa ecdsa:
                        cert = cert.CopyWithPrivateKey(ecdsa);
                        break;

                    case DSA dsa:
                        cert = cert.CopyWithPrivateKey(dsa);
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported key type: {privateKey?.GetType().Name}");
                }

                // save certificate
                certificateStore.LoadCertificateStore(certificateStoreSerializer, false);
                certificateStore.AddCertificate(config.Alias ?? cert.Thumbprint, Convert.ToBase64String(cert.Export(X509ContentType.Pfx)), config.Overwrite, null, RemoveRootCertificate);
                certificateStore.SaveCertificateStore(certificateStoreSerializer.SerializeRemoteCertificateStore(certificateStore.GetCertificateStore(), storePathFile.Path, storePathFile.File, StorePassword, certificateStore.RemoteHandler));

                logger.LogDebug($"END add Operation for {config.CertificateStoreDetails.StorePath} on {config.CertificateStoreDetails.ClientMachine}.");
            }

            catch (Exception ex)
            {
                string errorMessage = $"Exception for {config.Capability}: {RemoteFileException.FlattenExceptionMessages(ex, string.Empty)} for job id {config.JobId}";
                logger.LogError(errorMessage);
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: {errorMessage}" };
            }
            finally
            {
                if (certificateStore.RemoteHandler != null)
                    certificateStore.Terminate();
            }

            logger.LogDebug($"...End {config.Capability} job for job id {config.JobId}");
            return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }
    }
}
