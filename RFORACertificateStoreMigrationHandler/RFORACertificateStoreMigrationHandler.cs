using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using RemoteFileTypeHandlers.BaseMigration;

namespace RemoteFileTypeHandlers.RFORA
{
    public class RFORACertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
        public Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(
            IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots,
            CancellationToken cancellationToken)
        {
            return BaseCertificateStoreMigrationHandler.GetCertificateStoreTypeDifferencesAsync(
                storeTypeSnapshots,
                RFORACertificateStoreTypeFactory.CustomFields,
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
                RFORACertificateStoreTypeFactory.CustomFields,
                cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
