using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;

namespace RemoteFileTypeHandlers.RFDER
{
    internal class RFJKSCertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
        static readonly List<string> CertificateStoreTypePropertyDiff = new List<CertificateStoreTypePropertyDiff> 
        { "ServerUsername", "ServerPassword", "LinuxFilePermissionsOnStoreCreation", "LinuxFileOwnerOnStoreCreation", "SudoImpersonatedUser", , , ,  };

        /// <summary>
        /// This method is used to return custom fields that the client will need to add values for.  The collection of store type snapshots are store types, we return collections of parameters that we need the user to enter.
        /// This method may not be needed if we decide not to produce that screeen and/or not support downgrade of versions.
        /// </summary>
        /// <param name="storeTypeSnapshots"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots, CancellationToken cancellationToken)
        {
            List<MigrationParameterDelta> deltas = new List<MigrationParameterDelta>();

            foreach(CertificateStoreTypeSnapshot snapshot in storeTypeSnapshots)
            {
                snapshot
                MigrationParameterDelta delta = new MigrationParameterDelta();
                //delta.
                CertificateStoreTypePropertyDiff diff = new CertificateStoreTypePropertyDiff();
                diff.
                CertificateStoreTypeProperty prop = new CertificateStoreTypeProperty();
                prop.
            }
            
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MigrationResult>> MigrateAsync(IEnumerable<CertificateStoreSnapshot> certificateStores, IEnumerable<CertificateStorePropertyDiff> updatedProperties, CancellationToken cancellationToken)
        {
            foreach (CertificateStoreSnapshot snapShot in certificateStores)
            {
                foreach (CertificateStorePropertyDiff x in updatedProperties)
                {
                    snapShot.
                    MigrationResult y = new MigrationResult();
                    y.CertificateStore.pro
                }

            }
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
