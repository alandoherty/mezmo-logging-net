namespace Mezmo.Extensions.Logging
{
    /// <summary>
    /// Implements a 
    /// </summary>
    public sealed class MezmoLogBuilder
    {
        /// <summary>
        /// The API key.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// The URI, optional and uses client default otherwise.
        /// </summary>
        public Uri? Uri { get; set; }

        /// <summary>
        /// The client factory to use when creating the <see cref="HttpClient"/>, optional.
        /// </summary>
        public IHttpClientFactory? ClientFactory { get; set; }

        /// <summary>
        /// The application name to use in logs, optional.
        /// </summary>
        public string? AppName { get; set; }

        /// <summary>
        /// The tags, optional.
        /// </summary>
        public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Build the <see cref="MezmoLogProvider"/>
        /// </summary>
        /// <returns>The provider.</returns>
        internal MezmoLogProvider Build()
        {
            if (ApiKey == null) {
                throw new InvalidOperationException("The builder must be configured with an API key");
            }

            return new MezmoLogProvider(ApiKey, Uri, ClientFactory, AppName, Tags);
        }
    }
}