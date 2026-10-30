using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using System.Text.Json;

namespace RemoteFileTypeHandlers.BaseMigration
{
    public static class BaseCertificateStoreMigrationHandler
    {
        public static Task<IEnumerable<MigrationParameterDelta>> GetCertificateStoreTypeDifferencesAsync(
            IEnumerable<CertificateStoreTypeSnapshot> storeTypeSnapshots,
            IEnumerable<CertificateStoreTypeProperty> customFields,
            CancellationToken cancellationToken)
        {
            List<MigrationParameterDelta> deltas = new List<MigrationParameterDelta>();

            foreach (CertificateStoreTypeSnapshot snapshot in storeTypeSnapshots)
            {
                IEnumerable<CertificateStoreTypeProperty> missingProperties = snapshot.Properties
                    .Where(property => !customFields.Any(field => field.Name == property.Name));

                if (missingProperties.Any())
                {
                    deltas.Add(new MigrationParameterDelta()
                    {
                        CertificateStoreTypeId = snapshot.Id,
                        StoreTypeDifferences = ConvertToDifferences(missingProperties)
                    });
                }
            }

            return Task.FromResult((IEnumerable<MigrationParameterDelta>)deltas);
        }

        public static Task<IEnumerable<MigrationResult>> MigrateAsync(
            IEnumerable<CertificateStoreSnapshot> certificateStores,
            IEnumerable<CertificateStorePropertyDiff> updatedProperties,
            IEnumerable<CertificateStoreTypeProperty> customFields,
            CancellationToken cancellationToken)
        {
            List<MigrationResult> results = new List<MigrationResult>();

            foreach (CertificateStoreSnapshot snapshot in certificateStores)
            {
                Dictionary<string, object> snapshotProperties =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(snapshot.Properties)
                    ?? new Dictionary<string, object>();
                List<CertificateStorePropertyDiff> newProperties = updatedProperties
                    .Where(property => property.CertificateStoreId == snapshot.Id)
                    .ToList();

                foreach (CertificateStorePropertyDiff property in newProperties)
                {
                    snapshotProperties.Add(property.PropertyName, property.PropertyValue);
                }

                foreach (string key in snapshotProperties.Keys
                    .Where(key => !customFields.Any(field => field.Name == key))
                    .ToList())
                {
                    snapshotProperties.Remove(key);
                }

                snapshot.Properties = JsonSerializer.Serialize(snapshotProperties);
                results.Add(new MigrationResult()
                {
                    CertificateStore = snapshot,
                    Status = MigrationResultStatus.NeedsInventory
                });
            }

            return Task.FromResult((IEnumerable<MigrationResult>)results);
        }

        private static IEnumerable<CertificateStoreTypePropertyDiff> ConvertToDifferences(
            IEnumerable<CertificateStoreTypeProperty> properties)
        {
            return properties.Select(property => new CertificateStoreTypePropertyDiff()
            {
                Name = property.Name,
                DisplayName = property.DisplayName,
                PropertyDataType = property.Type,
                Required = property.ValidationOptions.OnCreation == CertificateStoreTypeValidationOptions.Required,
                DefaultValue = property.DefaultValue
            }).ToList();
        }
    }
}
