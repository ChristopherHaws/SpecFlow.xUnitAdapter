using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers.VsTest
{
    public class RunConfiguration
    {
        public string SettingsName { get; set; }

        public string TestAdaptersPaths { get; set; }

        public RunConfiguration(string testAdapterPaths)
        {
            this.SettingsName = Constants.RunConfigurationSettingsName;
            this.TestAdaptersPaths = testAdapterPaths;
        }

        public XmlElement ToXml()
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement(this.SettingsName);

            var testAdaptersPaths = doc.CreateElement("TestAdaptersPaths");
            testAdaptersPaths.InnerXml = this.TestAdaptersPaths;
            root.AppendChild(testAdaptersPaths);

            return root;
        }
    }
}
