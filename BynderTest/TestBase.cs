using System.Collections.Generic;
using inRiver.Remoting;
using inRiver.Remoting.Extension;
using inRiver.Remoting.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BynderTest
{
    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }
        protected inRiverContext InRiverContext;
        protected Logger Logger;

        protected Dictionary<string,string> TestSettings = new Dictionary<string, string>
        {
            {Bynder.Api.SettingNames.ConsumerKey, "FF593D1D-5F1A-4449-88280AF539EE9D99" },
            {Bynder.Api.SettingNames.ConsumerSecret, "7300085642fb8ce1443f0f27147c1382" },
            {Bynder.Api.SettingNames.CustomerBynderUrl, "https://sigvaris.getbynder.com" },
            {Bynder.Api.SettingNames.Token, "24DAECED-6996-4C23-99DB29C8EB527275" },
            {Bynder.Api.SettingNames.TokenSecret, "729b22c258f0349587bdfcac140b97b2" },
            {Bynder.Config.Settings.RegularExpressionForFileName, @"^(?<ProductNumber>[0-9a-zA-Z]+)_(?<ResourceType>image|document)_(?<ResourcePosition>[0-9]+)" },
            {Bynder.Config.Settings.InitialAssetLoadUrlQuery, @"type=image" },
            {Bynder.Config.Settings.MetapropertyMap, @"C7BC01E1-670D-4410-A7B81E9032FE261A=ResourcePosition,C284234B-29B6-4CA8-B907B728455F30EA=ProductNumber" }
        };

        [TestInitialize]
        public void TestInitialize()
        {
            Logger = new Logger(TestContext);
            Logger.Log(LogLevel.Information, $"Initialize connection to inRiver Server");
            InRiverContext = new inRiverContext(
                RemoteManager.CreateInstance("https://remoting.productmarketingcloud.com",
                    "sigvaris@avensia.com", "Abcd!234", "Test"), Logger);

            Assert.IsNotNull(InRiverContext, "Login failed ??");
        }

    }
}
