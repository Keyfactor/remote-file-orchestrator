using Keyfactor.Platform.ICertificateStoreTypeMigrationHandler;

namespace RemoteFileTypeHandlers.RFJKS
{
    public class RFJKSCertificateStoreTypeFactory : ICertificateStoreTypeFactory
    {
        /// <summary>
        /// This replaces the integration-manifest.  Returns an object representation of the store type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public CertificateStoreTypeSeed Create()
        {
            CertificateStoreTypeSeed x = new CertificateStoreTypeSeed();
            //x. = "RFDER"
            x.p

            CertificateStoreTypeEntryParameter y = new CertificateStoreTypeEntryParameter();
            y.

            throw new NotImplementedException();
        }
        /// <summary>
        /// probably not needed to be implemented, but need to remove the NotImplementedException, just leave empty.  This is a place holder in case we need to dispose something.
        /// </summary>
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
