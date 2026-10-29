using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using RemoteFileTypeHandlers.BaseMigration;

namespace RemoteFileTypeHandlers.RFPEM
{
    public class RFPEMCertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
        public Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(
            IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots,
            CancellationToken cancellationToken)
        {
            return BaseCertificateStoreMigrationHandler.GetCertificateStoreTypeDifferencesAsync(
                storeTypeSnapshots,
                RFPEMCertificateStoreTypeFactory.CustomFields,
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
                RFPEMCertificateStoreTypeFactory.CustomFields,
                cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
