using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers.VsTest
{
    public class DiscoveryEventsHandler : ITestDiscoveryEventsHandler
    {
        private readonly ILogger logger;

        public DiscoveryEventsHandler()
        {
        }

        public DiscoveryEventsHandler(ILogger logger)
            : this()
        {
            this.logger = logger;
        }

        public IList<TestCase> TestCases { get; } = new List<TestCase>();

        public void HandleDiscoveredTests(IEnumerable<TestCase> discoveredTestCases)
        {
            if (discoveredTestCases == null)
            {
                return;
            }

            foreach (var testCase in discoveredTestCases)
            {
                this.TestCases.Add(testCase);
            }
        }

        public void HandleDiscoveryComplete(long totalTests, IEnumerable<TestCase> lastChunk, bool isAborted)
        {
            if (lastChunk == null)
            {
                return;
            }

            foreach (var testCase in lastChunk)
            {
                this.TestCases.Add(testCase);
            }

            this.logger.Debug("Test Discovery Complete");
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            this.logger.Log(level, message);
        }

        public void HandleRawMessage(string rawMessage)
        {
            this.logger.Trace(rawMessage);
        }
    }
}
