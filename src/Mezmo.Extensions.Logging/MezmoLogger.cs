using System.Text.Json;
using Mezmo.Logging;
using Microsoft.Extensions.Logging;

namespace Mezmo.Extensions.Logging
{
    /// <summary>
    /// Implements an <see cref="ILogger{TCategoryName}"/> for Mezmo.
    /// </summary>
    class MezmoLogger : ILogger
    {
        private readonly MezmoLogProvider _provider;

        private string? _metaScope;
        private object _metaObj = new object();
        
        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Format the log line
            string line = formatter(state, exception);

            if (exception != null) {
                line = $"{line}{Environment.NewLine}{exception}";
            }
            
            // Convert to the Mezmo style log levels
            string level = "INFO";
            
            switch (logLevel) {
                case LogLevel.Warning:
                    level = "WARN";
                    break;
                case LogLevel.Information:
                    level = "INFO";
                    break;
                case LogLevel.Error:
                    level = "ERROR";
                    break;
                case LogLevel.Debug:
                    level = "DEBUG";
                    break;
                case LogLevel.Critical:
                    level = "CRIT";
                    break;
            }
            
            _provider.Client.Enqueue(new LogLine() {
                Level = level,
                Line = line,
                AppName = _provider.AppName,
                Timestamp = DateTimeOffset.UtcNow,
                Metadata = _metaScope
            });
        }
        
        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Trace)
                return false;
            
            return true;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            // Serialize the scope now (so we can use it multiple times)
            // If the state is a string we pass it directly
            string scopeSerialized;
            
            if (state is string str) {
                scopeSerialized = str;
            } else {
                scopeSerialized = JsonSerializer.Serialize(state);
            }
            
            lock (_metaObj) {
                if (_metaScope != null) {
                    throw new InvalidOperationException("The logger does not support nested scopes");
                }

                _metaScope = scopeSerialized;
            }

            return new LoggerScopeDisposable(this);
        }

        /// <summary>
        /// Implements an <see cref="IDisposable"/> that can clear the metadata scope.
        /// </summary>
        class LoggerScopeDisposable : IDisposable
        {
            private readonly MezmoLogger _logger;
            private bool _disposed;
            
            public void Dispose()
            {
                if (_disposed) {
                    return;
                }

                _disposed = true;
                _logger._metaScope = null;
            }

            public LoggerScopeDisposable(MezmoLogger logger)
            {
                _logger = logger;
            }
        }

        internal MezmoLogger(MezmoLogProvider provider)
        {
            _provider = provider;
        }
    }
}