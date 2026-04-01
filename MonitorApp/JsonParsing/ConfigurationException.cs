using System;

namespace MonitorApp.JsonParsing
{
    /// <summary>
    /// Represents errors that occur during configuration parsing.
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }

        public ConfigurationException(string message, Exception inner) : base(message, inner) { }
    }
}
