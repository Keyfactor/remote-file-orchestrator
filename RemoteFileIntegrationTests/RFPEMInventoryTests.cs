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
            CreateStore("Test0001", false, false);
            CreateStore("Test0002", false, true);
            CreateStore("Test0003", true, false);
            CreateStore("Test0004", true, true);
        }

        public override void TearDown()
        {
            throw new NotImplementedException();
        }

        
    }
}