using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers.VsTest
{
    public static class LoggerExtensions
    {
        public static void Log(this ILogger logger, TestMessageLevel level, string message)
        {
            switch (level)
            {
                case TestMessageLevel.Informational:
                    logger.Info(message);
                    break;
                case TestMessageLevel.Warning:
                    logger.Warning(message);
                    break;
                case TestMessageLevel.Error:
                    logger.Error(message);
                    break;
                default:
                    logger.Info(message);
                    break;
            }
        }
    }
}
