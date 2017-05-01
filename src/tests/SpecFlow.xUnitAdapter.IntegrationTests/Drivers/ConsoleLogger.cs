using System;

namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers
{
    public class ConsoleLogger : ILogger
    {
        public void Trace(string message) => this.Write("TRACE", message);
        public void Debug(string message) => this.Write("DEBUG", message);
        public void Info(string message) => this.Write("INFO", message);
        public void Warning(string message) => this.Write("WARN", message);
        public void Error(string message) => this.Write("ERROR", message);
        private void Write(string level, string message) => Console.WriteLine($"{DateTime.Now:HH:mm:ss.ffffff} - {level}: {message}");
    }
}
