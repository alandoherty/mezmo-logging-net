using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Channels;

namespace Mezmo.Logging
{
    /// <summary>
    /// Provides a HTTP based client for the Ingest API.
    /// </summary>
    public class IngestClient : IIngestClient
    {
        private const string DefaultApiUrl = "https://logs.logdna.com";
        private const int DefaultSendIntervalInMs = 250;
        private const int MaxBodyInBytes = 10000000;
        private const int MaxMessageInBytes = 16000;
        private const int MaxMetadataInBytes = 32000;
        private const int MaxHostnameLength = 256;
        private const int MaxAppNameLength = 512;
        private const int MaxLogLevel = 80;
        private const int MaxTags = 80;
        private const int MaxFieldDepth = 3;
        
        private static readonly MediaTypeHeaderValue ContentTypeJsonUtf8 = MediaTypeHeaderValue.Parse("application/json; charset=UTF-8");
            
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _disposeCancellationSource = new CancellationTokenSource();
        
        private int _disposed;
        private ConcurrentQueue<LogLine> _pendingLines = new ConcurrentQueue<LogLine>();
        private Task _loopTask;

        private object _flushObj = new();
        private TaskCompletionSource? _flushRequest = new TaskCompletionSource();
        private TaskCompletionSource? _flushCompletion = null;

        private string _cachedTags = "";
        private IEnumerable<string> _tags = Enumerable.Empty<string>();

        /// <inheritdoc/>
        public IEnumerable<string> Tags
        {
            get => _tags;
            set {
                _cachedTags = string.Join(",", value);
                _tags = value;
            }
        }

        /// <inheritdoc/>
        public TimeSpan SendInterval { get; set; } = TimeSpan.FromMilliseconds(DefaultSendIntervalInMs);

        /// <inheritdoc/>
        public string ApiKey { get; }

        /// <inheritdoc/>
        public HttpClient Client => _client;
        
        /// <inheritdoc/>
        public void Enqueue(LogLine line)
        {
            if (_disposed > 0) throw new ObjectDisposedException("The ingest client has been disposed");

            _pendingLines.Enqueue(line);
        }

        /// <inheritdoc/>
        public void EnqueueRange(IEnumerable<LogLine> lines)
        {
            if (_disposed > 0) throw new ObjectDisposedException("The ingest client has been disposed");

            foreach (var line in _pendingLines) {
                _pendingLines.Enqueue(line);
            }
        }

        /// <summary>
        /// Sends pending log lines periodically.
        /// </summary>
        private async Task SendLoopAsync()
        {
            while (!_disposeCancellationSource.IsCancellationRequested || _pendingLines.Count > 0) {
                // Wait the send interval (or until we get disposed)
                if (!_disposeCancellationSource.IsCancellationRequested) {
                    try {
                        await Task.Delay(SendInterval, _disposeCancellationSource.Token).ConfigureAwait(false);
                    } catch (OperationCanceledException) {
                    }
                }

                TaskCompletionSource? completionSource = null;

                lock (_flushObj) {
                    if (_flushCompletion != null) {
                        completionSource = _flushCompletion;
                        _flushCompletion = null;
                        _flushRequest = null;
                    }
                }
                
                // If we have no log lines we can skip sending anything
                if (_pendingLines.Count == 0)
                    continue;
                
                // Send log lines
                using (MemoryStream ms = new MemoryStream())
                using (Utf8JsonWriter jw = new Utf8JsonWriter(ms)) {
                    jw.WriteStartObject();
                    jw.WriteStartArray("lines");
                    
                    while (_pendingLines.Count > 0) {
                        if (_pendingLines.TryDequeue(out LogLine? line)) {
                            JsonSerializer.Serialize(jw, line);
                            continue;
                        }

                        break;
                    }
                    
                    jw.WriteEndArray();
                    jw.WriteEndObject();
                    jw.Flush();

                    // Seek back to beginning
                    ms.Seek(0, SeekOrigin.Begin);

                    try {
                        // Build the content payload
                        var content = new StreamContent(ms);
                        content.Headers.ContentType = ContentTypeJsonUtf8;
                        
                        // Build the query
                        string requestUri =
                            $"logs/ingest?hostname={Environment.MachineName}&now={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&tags={_cachedTags}";
                            
                        // Post the log payload
                        using (var response = await _client.PostAsync(requestUri, content)
                                   .ConfigureAwait(false)) {
                            response.EnsureSuccessStatusCode();
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine("Exception occured ingesting log: {0}", ex.ToString());
                    }
                }
            }
        }
        
        /// <summary>
        /// Flushes any pending log lines and disposes of the ingestion client.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public async ValueTask DisposeAsync()
        {
            // Atomically mark the client as disposed
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) {
                return;
            }

            // Wait for loop task to end
            try {
                _disposeCancellationSource.Cancel();
                await _loopTask.ConfigureAwait(false);
            } finally {
                Client.Dispose();
            }
        }

        /// <summary>
        /// Creates a new ingestion client with the default API URI.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        public IngestClient(string apiKey)
            : this(apiKey, (Uri?)null)
        {  
        }
        
        /// <summary>
        /// Creates a new ingestion client with the specified URI.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="uri">The API URI.</param>
        public IngestClient(string apiKey, Uri? uri)
        {
            ApiKey = apiKey;
            _client = new HttpClient();
            _client.BaseAddress = uri ?? new Uri(DefaultApiUrl);
            _client.DefaultRequestHeaders.Add("apikey", apiKey);
            _loopTask = SendLoopAsync();
        }
        
        /// <summary>
        /// Creates a new ingestion client from the provided factory using the default API URI.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="clientFactory">The client factory..</param>
        public IngestClient(string apiKey, IHttpClientFactory? clientFactory)
            : this(apiKey, null, clientFactory)
        {
        }
        
        /// <summary>
        /// Creates a new ingestion client from the provided factory using the specified URI.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="uri">The API URI.</param>
        /// <param name="clientFactory">The client factory.</param>
        public IngestClient(string apiKey, Uri? uri, IHttpClientFactory? clientFactory)
        {
            ApiKey = apiKey;
            _client = clientFactory == null ? new HttpClient() : clientFactory.CreateClient();
            _client.BaseAddress = uri ?? new Uri(DefaultApiUrl);
            _client.DefaultRequestHeaders.Add("apikey", apiKey);
            _loopTask = SendLoopAsync();
        }
    }
}
