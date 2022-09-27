using Example.Hosting.Configuration;
using Mezmo.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.Hosting;

public static class Program
{
    /// <summary>
    /// The entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c => {
                c.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Settings.json", optional:true)
                    .AddEnvironmentVariables()
                    .Build();
            })
            .ConfigureServices(ConfigureServices)
            .Build()
            .Run();
    }
        
    /// <summary>
    /// Configures services on the application.
    /// </summary>
    static void ConfigureServices(HostBuilderContext ctx, IServiceCollection serviceCollection)
    {
        // Configure logging
        serviceCollection.AddLogging(b =>
        {
            // Add LogDNA is a key is available
            MezmoOptions mezmoOptions = ctx.Configuration.GetSection("Mezmo").Get<MezmoOptions>();
            
            if (mezmoOptions.ApiKey != null) {
                b.AddMezmo(mb =>
                {
                    mb.Uri = mezmoOptions.Uri == null ? null : new Uri(mezmoOptions.Uri);
                    mb.ApiKey = mezmoOptions.ApiKey;
                    mb.AppName = mezmoOptions.AppName;
                    mb.Tags = mezmoOptions.Tags.Split(',').Select(t => t.Trim()).ToArray();
                });
            }
            
            b.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });
        
        // Configure a basic service that does some logging
        serviceCollection.AddHostedService<Worker>();
    }
}