namespace Example.Hosting.Configuration
{
    /// <summary>
    /// Represents options for Mezmo.
    /// </summary>
    public record MezmoOptions
    {
        /// <summary>
        /// The API key for ingesting logs, required.
        /// </summary>
        public string? ApiKey { get; set; }
        
        /// <summary>
        /// The application name, optional.
        /// </summary>
        public string? AppName { get; set; }
        
        /// <summary>
        /// The custom ingest API URL, optional.
        /// </summary>
        public string? Uri { get; set; }

        /// <summary>
        /// The comma-seperated tags, optional.
        /// </summary>
        public string Tags { get; set; } = "";
    }
}