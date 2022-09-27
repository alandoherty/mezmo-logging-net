using System.Text.Json.Serialization;

namespace Mezmo.Logging
{
    /// <summary>
    /// Represents a single log line.
    /// </summary>
    public record LogLine
    {
        /// <summary>
        /// The timestamp of the log, optional and defaults to construction.
        /// </summary>
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTimeOffsetTimestampConverter))]
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The log message.
        /// </summary>
        [JsonPropertyName("line")]
        public string? Line { get; init; }
        
        /// <summary>
        /// The name of the application, optional.
        /// </summary>
        [JsonPropertyName("app")]
        public string? AppName { get; init; }

        /// <summary>
        /// The log level, optional and defaults to <c>INFO</c>.
        /// </summary>
        [JsonPropertyName("level")]
        public string? Level { get; init; }

        /// <summary>
        /// The log line metadata.
        /// </summary>
        [JsonPropertyName("meta")]
        public string? Metadata { get; init; }
    }
}