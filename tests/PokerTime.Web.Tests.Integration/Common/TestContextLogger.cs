namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    public sealed class TestContextLoggerProvider : ILoggerProvider {
        public void Dispose() => TestContext.WriteLine($"{typeof(TestContextLogger)}: Dispose");

        public ILogger CreateLogger(string categoryName) => new TestContextLogger(categoryName);
    }

    internal sealed class TestContextLogger : ILogger {
        private readonly string _categoryName;
        private string _scopeName;

        public TestContextLogger(string categoryName) {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) {
            _scopeName = state?.ToString();
            return new LoggingScope(this);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        ) {
            var preamble =
                $"[{_categoryName}] {(!string.IsNullOrEmpty(_scopeName) ? (_scopeName + " -> ") : "")}";

            TestContext.WriteLine($"{preamble} {eventId} {formatter(state, exception)} [{exception}]");
        }

        private sealed class LoggingScope : IDisposable {
            private readonly TestContextLogger _parent;

            public LoggingScope(TestContextLogger parent) {
                _parent = parent;
            }

            public void Dispose() => _parent._scopeName = null;
        }
    }
}
