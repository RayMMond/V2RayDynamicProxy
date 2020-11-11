using System;
using Microsoft.Extensions.Logging;

namespace DynamicProxy
{
    public class ConsoleLogger<T> : ILogger<T>
    {
        public static readonly ConsoleLogger<T> Instance = new ConsoleLogger<T>();

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;

        /// <inheritdoc />
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(
                $"{DateTime.Now} {logLevel} {eventId} {state} {exception} {formatter?.Invoke(state, exception)}");
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => false;

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();

            public void Dispose()
            {
            }
        }
    }
}