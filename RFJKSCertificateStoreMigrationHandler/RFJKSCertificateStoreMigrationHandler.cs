using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;
using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteFileTypeHandlers.RFJKS
{
    internal class RFJKSCertificateStoreMigrationHandler : ICertificateStoreMigrationHandler
    {
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
            IEnumerable<MigrationParameterDelta> deltas = new List<MigrationParameterDelta>();

            foreach(CertificateStoreTypeSnapshot snapshot in storeTypeSnapshots)
            {
                IEnumerable<CertificateStoreTypeProperty> missingProperties = snapshot.Properties.Where(p => !RFJKSCertificateStoreTypeFactory.CustomFields.Any(f => f.Name == p.Name));
                if (missingProperties != null && missingProperties.Count() > 0)
                {
                    MigrationParameterDelta delta = new MigrationParameterDelta()
                    {
                        CertificateStoreTypeId = snapshot.Id,
                        StoreTypeDifferences = ConvertToDifferences(missingProperties)
                    };
                    deltas = deltas.Append(delta);
                }
            }   

            return Task.FromResult(deltas);
        }

        private IEnumerable<CertificateStoreTypePropertyDiff> ConvertToDifferences(IEnumerable<CertificateStoreTypeProperty> missingProperties)
        {
            List<CertificateStoreTypePropertyDiff> diffs = new List<CertificateStoreTypePropertyDiff>();

            foreach(CertificateStoreTypeProperty p in missingProperties)
            {
                CertificateStoreTypePropertyDiff diff = new CertificateStoreTypePropertyDiff()
                {
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    PropertyDataType = p.Type,
                    Required = p.ValidationOptions.OnCreation == CertificateStoreTypeValidationOptions.Required,
                    DefaultValue = p.DefaultValue
                };
                diffs.Add(diff);
            }

            return diffs;
        }

        public Task<IEnumerable<MigrationResult>> MigrateAsync(IEnumerable<CertificateStoreSnapshot> certificateStores, IEnumerable<CertificateStorePropertyDiff> updatedProperties, CancellationToken cancellationToken)
        {
            List<MigrationResult> results = new List<MigrationResult>();

            foreach (CertificateStoreSnapshot snapShot in certificateStores)
            {
                Dictionary<string, object> snapShotProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(snapShot.Properties) ?? new Dictionary<string, object>();
                List<CertificateStorePropertyDiff> newProperties = updatedProperties.Where(p => p.CertificateStoreId == snapShot.Id).ToList();

                foreach(CertificateStorePropertyDiff diff in newProperties)
                    snapShotProperties.Add(diff.PropertyName, diff.PropertyValue);

                foreach (var key in snapShotProperties.Keys.Where(p => !RFJKSCertificateStoreTypeFactory.CustomFields.Exists(q => q.Name == p)))
                {
                    snapShotProperties.Remove(key);
                }
                snapShot.Properties = JsonSerializer.Serialize(snapShotProperties);

                results.Add(new MigrationResult()
                {
                    CertificateStore = snapShot,
                    Status = MigrationResultStatus.Success
                });
            }

            return Task.FromResult(((IEnumerable<MigrationResult>)results));
        }

        public void Dispose()
        {
        }
    }
}
