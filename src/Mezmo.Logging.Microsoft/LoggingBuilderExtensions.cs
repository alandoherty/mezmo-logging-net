using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mezmo.Logging.Microsoft
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Configures and adds a Mezmo log provider.
        /// </summary>
        /// <param name="builder">The logging builder.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The logging builder.</returns>
        public static ILoggingBuilder AddMezmo(this ILoggingBuilder builder, string apiKey)
        {
            MezmoLogBuilder mezmoBuilder = new MezmoLogBuilder();
            mezmoBuilder.ApiKey = apiKey;
            builder.AddProvider(mezmoBuilder.Build());
            
            return builder;
        }
        
        /// <summary>
        /// Configures and adds a Mezmo log provider.
        /// </summary>
        /// <param name="builder">The logging builder.</param>
        /// <param name="builderAction">The Mezmo builder action.</param>
        /// <returns>The logging builder.</returns>
        public static ILoggingBuilder AddMezmo(this ILoggingBuilder builder, Action<MezmoLogBuilder> builderAction)
        {
            MezmoLogBuilder mezmoBuilder = new MezmoLogBuilder();
            builderAction(mezmoBuilder);
            builder.AddProvider(mezmoBuilder.Build());
            
            return builder;
        }
    }
}