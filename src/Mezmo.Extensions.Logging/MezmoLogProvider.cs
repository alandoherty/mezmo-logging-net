using Mezmo.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mezmo.Extensions.Logging
{
    /// <summary>
    /// The Mezmo log provider.
    /// </summary>
    public class MezmoLogProvider : ILoggerProvider
    {
        private readonly IngestClient _client;
        private readonly string? _appName;

        /// <summary>
        /// Gets the ingestion client.
        /// </summary>
        internal IngestClient Client => _client;

        /// <summary>
        /// Gets the application name, if any.
        /// </summary>
        public string? AppName => _appName;

        /// <summary>
        /// Dispose the log provider.
        /// </summary>
        public void Dispose()
        {
            _client.DisposeAsync()
                .AsTask()
                .Wait();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new MezmoLogger(this);
        }

        internal MezmoLogProvider(string apiKey, Uri? uri, IHttpClientFactory? clientFactory, string? appName, IEnumerable<string> tags,
            TimeSpan? sendInterval, 
            int? sendBufferCapacity)
        {
            _appName = appName;
            _client = new IngestClient(apiKey, uri, clientFactory);
            _client.Tags = tags;
            if (sendInterval != null) _client.SendInterval = sendInterval.Value;
            if (sendBufferCapacity != null) _client.SendBufferCapacity = sendBufferCapacity.Value;
        }
    }
}