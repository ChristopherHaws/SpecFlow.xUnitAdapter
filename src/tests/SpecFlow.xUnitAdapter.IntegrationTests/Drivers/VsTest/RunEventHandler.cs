using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers.VsTest
{
    public class RunEventHandler : ITestRunEventsHandler
    {
        private readonly AutoResetEvent waitHandle;
        private readonly ILogger logger;

        public List<TestResult> TestResults { get; } = new List<TestResult>();

        public RunEventHandler(ILogger logger, AutoResetEvent waitHandle)
        {
            this.waitHandle = waitHandle;
            this.logger = logger;
        }

        public void HandleLogMessage(TestMessageLevel level, string message)
        {
            this.logger.Log(level, "Run Message: " + message);
        }

        public void HandleTestRunComplete(
            TestRunCompleteEventArgs testRunCompleteArgs,
            TestRunChangedEventArgs lastChunkArgs,
            ICollection<AttachmentSet> runContextAttachments,
            ICollection<string> executorUris)
        {
            if (lastChunkArgs != null && lastChunkArgs.NewTestResults != null)
            {
                this.TestResults.AddRange(lastChunkArgs.NewTestResults);
            }

            this.logger.Debug("TestRunComplete");
            this.waitHandle.Set();
        }

        public void HandleTestRunStatsChange(TestRunChangedEventArgs testRunChangedArgs)
        {
            if (testRunChangedArgs != null && testRunChangedArgs.NewTestResults != null)
            {
                this.TestResults.AddRange(testRunChangedArgs.NewTestResults);
            }
        }

        public void HandleRawMessage(string rawMessage)
        {
            this.logger.Trace(rawMessage);
        }

        public int LaunchProcessWithDebuggerAttached(TestProcessStartInfo testProcessStartInfo)
        {
            // No op
            return -1;
        }
    }
}
