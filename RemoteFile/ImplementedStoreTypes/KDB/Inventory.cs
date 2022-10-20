﻿
namespace Keyfactor.Extensions.Orchestrator.RemoteFile.KDB
{
    public class Inventory : InventoryBase
    {
        internal override ICertificateStoreSerializer GetCertificateStoreSerializer()
        {
            return new KDBCertificateStoreSerializer();
        }
    }
}