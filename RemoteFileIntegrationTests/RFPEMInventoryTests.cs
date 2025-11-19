namespace RemoteFileIntegrationTests
{
    public class RFPEMInventoryTests : BaseRFPEMTest
    {
        [Fact(DisplayName = "RFPEM Inventory Internal Private Key")]
        public void RFPEM_Inventory_InternalPrivateKey_Test0001()
        {

        }

        public override void SetUp()
        {
            CreateStore("Test0001", false, false, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0002", false, true, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0003", true, false, STORE_ENVIRONMENT_ENUM.LINUX);
            CreateStore("Test0004", true, true, STORE_ENVIRONMENT_ENUM.LINUX);
        }

        public override void TearDown()
        {
            RemoveStore("Test0001", false, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0002", false, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0003", true, STORE_ENVIRONMENT_ENUM.LINUX);
            RemoveStore("Test0004", true, STORE_ENVIRONMENT_ENUM.LINUX);
        }
    }
}