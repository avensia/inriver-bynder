using Bynder.Api;
using Bynder.Api.Model;
using Bynder.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BynderTest
{
    [TestClass]
    public class ApiTest : TestBase
    {
        private const string _testAssetId = "73843ABB-B585-40C3-A9E217C9C06CD23C";
        private const string _testIntegrationId = "41a92562-bfd9-4847-a34d-4320bcef5e4a";

        private readonly BynderClientSettings _bynderSettings = new BynderClientSettings()
        {
            ConsumerKey = "94BE8A9A-C207-48E1-8CB9D21024CAC20D",
            ConsumerSecret = "bcc5d2feaa2d678ef75993a814792f20",
            CustomerBynderUrl = "https://assets.sigvaris.com",
            Token = "C36D7718-E23D-4B1D-BC291F7A7ED7E662",
            TokenSecret = "a87d4efa6677a8d9986b1d25b2d3b110"
        };
                
        [TestMethod]
        public void TestFlow()
        {
            GetAccount();
            CreateAssetUsage();
            GetAssetByAssetId();
            PostMetaProperties();
            GetAssetCollection();
            DeleteAssetUsage();
        }

        [Ignore("Add valid entity id here")]
        [DataRow(123)]
        [DataTestMethod]
        public void UploadEntityTest(int entityId)
        {
            Uploader uploader = new Uploader() { Context = InRiverContext };
            uploader.Context.Settings = TestSettings;
            uploader.EntityUpdated(entityId, null);
        }

        public void CreateAssetUsage()
        {
            BynderClient bynderBynderClient = new BynderClient(_bynderSettings);
            var result = bynderBynderClient.CreateAssetUsage(_testAssetId, _testIntegrationId, "http://test.com/123");
            Logger.Log(result);
        }

        public void DeleteAssetUsage()
        {
            BynderClient bynderBynderClient = new BynderClient(_bynderSettings);
            var result = bynderBynderClient.DeleteAssetUsage(_testAssetId, _testIntegrationId);
            Logger.Log(result);
        }

        public void PostMetaProperties()
        {
            BynderClient bynderBynderClient = new BynderClient(_bynderSettings);
            var mpl = new MetapropertyList()
            {
                new Metaproperty("50B5233E-AD1C-4CF5-82B910BADA62F30F", "Hello"),
                new Metaproperty("C284234B-29B6-4CA8-B907B728455F30EA", "World")
            };
            var result = bynderBynderClient.SetMetaProperties(_testAssetId, mpl);
            Logger.Log(result);
        }

        public void GetAssetByAssetId() { 
            BynderClient bynderClient = new BynderClient(_bynderSettings);
            Asset asset = bynderClient.GetAssetByAssetId(_testAssetId);
            var originalFileName = asset.GetOriginalFileName();
            Logger.Log(originalFileName);

            Assert.AreNotEqual(string.Empty, originalFileName, "Got no result");
        }

        public void GetAccount()
        {
           BynderClient bynderClient = new BynderClient(_bynderSettings);
            var accountName = bynderClient.GetAccount().Name;

            Logger.Log(accountName);
            Assert.IsNotNull(accountName);
        }

        public void GetAssetCollection()
        {
            BynderClient bynderClient = new BynderClient(_bynderSettings);
            var collection = bynderClient.GetAssetCollection("");
            Assert.IsInstanceOfType(collection, typeof(AssetCollection));
            Logger.Log("Total assets in result: " + collection.Total.Count);
        }
    }
}
