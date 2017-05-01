using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers.VsTest
{
    public class VsTestDriver : IDisposable
    {
        private readonly string adapterPath;
        private readonly string vstestConsolePath;
        private readonly VsTestConsoleWrapper vsTestConsoleWrapper;
        private readonly ILogger logger;

        public VsTestDriver(ILogger logger)
        {
            this.logger = logger;
            this.vsTestConsoleWrapper = new VsTestConsoleWrapper(this.GetConsoleRunnerPath());
            this.vsTestConsoleWrapper.StartSession();
        }

        public VsTestDriver(ILogger logger, string adapterPath)
            : this(logger)
        {
            this.adapterPath = adapterPath;
        }

        public VsTestDriver(ILogger logger, string adapterPath, string vstestConsolePath)
            : this(logger, adapterPath)
        {
            this.vstestConsolePath = vstestConsolePath;
        }

        public void Dispose()
        {
            this.vsTestConsoleWrapper?.EndSession();
        }

        public IList<TestCase> DiscoverTests(params string[] sources)
        {
            return this.DiscoverTests(sources, runSettings: null);
        }

        public IList<TestCase> DiscoverTests(string[] sources, string runSettings = "")
        {
            for (var iterator = 0; iterator < sources.Length; iterator++)
            {
                if (!Path.IsPathRooted(sources[iterator]))
                {
                    sources[iterator] = this.GetAssetFullPath(sources[iterator]);
                }
            }

            var discoveryEventsHandler = new DiscoveryEventsHandler(this.logger);
            var runSettingXml = this.GetRunSettingXml(runSettings, this.GetTestAdapterPath());

            this.vsTestConsoleWrapper.InitializeExtensions(Directory.GetFiles(this.GetTestAdapterPath(), "*TestAdapter.dll"));
            this.vsTestConsoleWrapper.DiscoverTests(sources, runSettingXml, discoveryEventsHandler);

            return discoveryEventsHandler.TestCases;
        }

        public IList<TestResult> RunTests(string[] sources, string[] names)
        {
            var tests = this.DiscoverTests(sources);

            tests = tests.Where(x => names.ToList().Contains(x.DisplayName)).ToList();

            return RunTests(tests);
        }

        public IList<TestResult> RunTests(IList<TestCase> tests)
        {
            var waiter = new AutoResetEvent(false);

            var handler = new RunEventHandler(this.logger, waiter);
            this.vsTestConsoleWrapper.RunTests(tests, null, handler);

            waiter.WaitOne();

            return handler.TestResults;
        }

        protected virtual string GetConsoleRunnerPath()
        {
            if (this.vstestConsolePath != null)
            {
                return this.vstestConsolePath;
            }

            // If vstest.console.exe is the host process, use that vstest
            var executingLocation = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var runnerLocation = Path.Combine(executingLocation, "vstest.console.exe");

            if (File.Exists(runnerLocation))
            {
                return runnerLocation;
            }
            
            IEnumerable<string> GetConsoleRunnerPaths()
            {
                var programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles");

                // VS2017 and Preview do not have a common tools environment variables because VS is getting more portable
                foreach (var previewEdition in new[] { "Enterprise", "Professional", "Community" })
                {
                    yield return Path.Combine(programFilesPath, "Microsoft Visual Studio", "Preview", previewEdition, "Common7", "IDE", "Extensions", "TestPlatform", "vstest.console.exe");
                }

                foreach (var vs2017Edition in new[] { "Enterprise", "Professional", "Community" })
                {
                    yield return Path.Combine(programFilesPath, "Microsoft Visual Studio", "2017", vs2017Edition, "Common7", "IDE", "Extensions", "TestPlatform", "vstest.console.exe");
                }

                yield return Path.Combine(Environment.GetEnvironmentVariable("VS140COMNTOOLS") ?? String.Empty, "IDE", "CommonExtensions", "Microsoft", "TestWindow", "vstest.console.exe");
                yield return Path.Combine(Environment.GetEnvironmentVariable("VS130COMNTOOLS") ?? String.Empty, "IDE", "CommonExtensions", "Microsoft", "TestWindow", "vstest.console.exe");
                yield return Path.Combine(Environment.GetEnvironmentVariable("VS140COMNTOOLS") ?? String.Empty, "IDE", "CommonExtensions", "Microsoft", "TestWindow", "vstest.console.exe");
            }

            foreach (var vsTestConsolePath in GetConsoleRunnerPaths())
            {
                if (File.Exists(vsTestConsolePath))
                {
                    return vsTestConsolePath;
                }
            }

            throw new Exception("vstest.console.exe could not be found.");
        }

        protected virtual string GetTestAdapterPath()
        {
            return this.adapterPath ?? Environment.CurrentDirectory;
        }

        protected virtual string GetAssetFullPath(string asset)
        {
            return Path.Combine(this.adapterPath ?? Environment.CurrentDirectory, asset);
        }

        protected virtual string GetRunSettingXml(string settingsXml, string testAdapterPath)
        {
            if (string.IsNullOrEmpty(settingsXml))
            {
                settingsXml = (XmlRunSettingsUtilities.CreateDefaultRunSettings() is XmlDocument x) ? x.OuterXml : String.Empty;
            }

            var doc = new XmlDocument();
            using (var xmlReader = XmlReader.Create(new StringReader(settingsXml), new XmlReaderSettings() { XmlResolver = null, CloseInput = true }))
            {
                doc.Load(xmlReader);
            }

            var root = doc.DocumentElement;
            var runConfiguration = new RunConfiguration(testAdapterPath);
            var runConfigElement = runConfiguration.ToXml();

            if (root[runConfiguration.SettingsName] == null)
            {
                var newNode = doc.ImportNode(runConfigElement, true);
                root.AppendChild(newNode);
            }
            else
            {
                var newNode = doc.ImportNode(runConfigElement.FirstChild, true);
                root[runConfiguration.SettingsName].AppendChild(newNode);
            }

            return doc.OuterXml;
        }
    }
}
