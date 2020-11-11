using System;
using Microsoft.Extensions.Logging;

namespace DynamicProxy
{
    public class ConsoleLoggerFactory : ILoggerFactory, IDisposable
    {
        /// <summary>
        /// Returns the shared instance of <see cref="T:Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory" />.
        /// </summary>
        public static readonly ConsoleLoggerFactory Instance = new ConsoleLoggerFactory();

        /// <inheritdoc />
        /// <remarks>
        /// This returns a <see cref="T:Microsoft.Extensions.Logging.Abstractions.NullLogger" /> instance which logs nothing.
        /// </remarks>
        public ILogger CreateLogger(string name) => (ILogger) ConsoleLogger<Program>.Instance;

        /// <inheritdoc />
        /// <remarks>This method ignores the parameter and does nothing.</remarks>
        public void AddProvider(ILoggerProvider provider)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}