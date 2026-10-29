using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using RemoteFileTypeHandlers.BaseMigration;

namespace RemoteFileTypeHandlers.RFJKS
{
    public class RFJKSCertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
        public Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(
            IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots,
            CancellationToken cancellationToken)
        {
            return BaseCertificateStoreMigrationHandler.GetCertificateStoreTypeDifferencesAsync(
                storeTypeSnapshots,
                RFJKSCertificateStoreTypeFactory.CustomFields,
                cancellationToken);
        }

        public Task<IEnumerable<MigrationResult>> MigrateAsync(
            IEnumerable<CertificateStoreSnapshot> certificateStores,
            IEnumerable<CertificateStorePropertyDiff> updatedProperties,
            CancellationToken cancellationToken)
        {
            return BaseCertificateStoreMigrationHandler.MigrateAsync(
                certificateStores,
                updatedProperties,
                RFJKSCertificateStoreTypeFactory.CustomFields,
                cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
