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

        // todo: Add your settings here
        protected Dictionary<string,string> TestSettings = new Dictionary<string, string>
        {
            {Bynder.Api.SettingNames.ConsumerKey, "94BE8A9A-C207-48E1-8CB9D21024CAC20D" },
            {Bynder.Api.SettingNames.ConsumerSecret, "bcc5d2feaa2d678ef75993a814792f20" },
            {Bynder.Api.SettingNames.CustomerBynderUrl, "https://assets.sigvaris.com" },
            {Bynder.Api.SettingNames.Token, "C36D7718-E23D-4B1D-BC291F7A7ED7E662" },
            {Bynder.Api.SettingNames.TokenSecret, "a87d4efa6677a8d9986b1d25b2d3b110" },
            {Bynder.Config.Settings.RegularExpressionForFileName, @"^(?<ProductNumber>[0-9a-zA-Z]+)_(?<ResourceType>image|document)_(?<ResourcePosition>[0-9]+)" },
            {Bynder.Config.Settings.InitialAssetLoadUrlQuery, @"tags=PIM" },
            {Bynder.Config.Settings.MetapropertyMap, @"" },
            {Bynder.Config.Settings.bynderBrandName, "" }
        };

        [TestInitialize]
        public void TestInitialize()
        {
            Logger = new Logger(TestContext);
            Logger.Log(LogLevel.Information, $"Initialize connection to inRiver Server");

            InRiverContext = new inRiverContext(
                RemoteManager.CreateInstance("https://remoting.productmarketingcloud.com",
                    "sigvaris@avensia.com", "Abcd!234", "test"), Logger);

            Assert.IsNotNull(InRiverContext, "Login failed ??");
        }
    }
}
