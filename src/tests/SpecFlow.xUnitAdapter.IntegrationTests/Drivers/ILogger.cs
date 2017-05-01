﻿namespace SpecFlow.xUnitAdapter.IntegrationTests.Drivers
{
    public interface ILogger
    {
        void Trace(string message);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}
