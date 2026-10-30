using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using Keyfactor.RemoteFileTypeandlers.BaseMigration;

namespace Keyfactor.RemoteFileTypeandlers.RFDER
{
    public class RFDERCertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
        public Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(
            IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots,
            CancellationToken cancellationToken)
        {
            return BaseCertificateStoreMigrationHandler.GetCertificateStoreTypeDifferencesAsync(
                storeTypeSnapshots,
                RFDERCertificateStoreTypeFactory.CustomFields,
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
                RFDERCertificateStoreTypeFactory.CustomFields,
                cancellationToken);
        }

        public void Dispose()
        {
        }
    }
}
