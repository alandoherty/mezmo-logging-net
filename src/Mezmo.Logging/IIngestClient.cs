namespace Mezmo.Logging
{
    /// <summary>
    /// Defines the interface for the Ingest API.
    /// </summary>
    public interface IIngestClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets the tags used when sending log lines, optional.
        /// </summary>
        /// <remarks>This value is cached and the underlying enumerable should not be modified after set</remarks>
        IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Gets or sets the send interval for batching log messages.
        /// </summary>
        TimeSpan SendInterval { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum number of lines to buffer before dropping log data.
        /// </summary>
        public int SendBufferCapacity { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="HttpClient"/> object used by the ingest client.
        /// </summary>
        HttpClient Client { get; }

        /// <summary>
        /// Queues a single log line to be sent.
        /// </summary>
        /// <param name="line">The log line.</param>
        void Enqueue(LogLine line);
        
        /// <summary>
        /// Queues a range of log lines to be sent.
        /// </summary>
        /// <param name="lines">The log lines.</param>
        void EnqueueRange(IEnumerable<LogLine> lines);
    }
}