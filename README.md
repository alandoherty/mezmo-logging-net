<div align="center">

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/alandoherty/mezmologging-net/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/alandoherty/mezmologging-net.svg?style=flat-square)](https://github.com/alandoherty/mezmologging-net/issues)
[![GitHub stars](https://img.shields.io/github/stars/alandoherty/mezmologging-net.svg?style=flat-square)](https://github.com/alandoherty/mezmologging-net/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/alandoherty/mezmologging-net.svg?style=flat-square)](https://github.com/alandoherty/mezmologging-net/network)
[![NuGet 1](https://img.shields.io/nuget/dt/Mezmo.Logging.svg?style=flat-square)](https://www.nuget.org/packages/Mezmo.Logging/)
[![NuGet 2](https://img.shields.io/nuget/dt/Mezmo.Logging.Microsoft.svg?style=flat-square)](https://www.nuget.org/packages/Mezmo.Logging.Microsoft/)

</div>

# mezmologging

A library for ingesting logs into Mezmo (formerly LogDNA) on .NET 6.0 and greater. The library provides high performance logging, and focuses on reducing overheads such as allocations.

Both a lower level API `Mezmo.Logging.IngestClient` and a higher level `Microsoft.Extensions.Logging` provider are available. The lower level API will batch logs, the interval of batching can be configured on `IIngestClient.SendInterval`. Any exceptions which occur while sending logs will be printed in the debug output.

You should ensure that you call `IIngestClient.DisposeAsync` to flush any remaining logs before exiting your application. This will be done automatically by the application when using `Microsoft.Extensions.Hosting` and configuring the log provider properly.

## Getting Started

[![NuGet Status](https://img.shields.io/nuget/v/Mezmo.Logging.Microsoft.svg?style=flat-square)](https://www.nuget.org/packages/Mezmo.Logging.Microsoft/)

You can install the package using either the CLI:

```
dotnet add package Mezmo.Logging
or
dotnet add package Mezmo.Logging.Microsoft
```

or from the NuGet package manager:

```
Install-Package Mezmo.Logging
or
Install-Package Mezmo.Logging.Microsoft
```

### Example

You can find an example setup usable with any `Microsoft.Hosting.Extensions` based application, including `ASP.NET Core`. This will not automatically record request based information, see the Metadata section for how to send log metadata.

You can configure the example with a `Settings.json` file, or by using the following environment variables.

| Name           | Example                          | Description                                     |
|----------------|----------------------------------|-------------------------------------------------|
| MEZMO__APIKEY  | xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx | Account ingestion key, required                 |
| MEZMO__TAGS    | mytag1, mytag2                   | Tags to apply to all logs, optional             |
| MEZMO__APPNAME | myapp                            | Application name to apply to all logs, optional |
| MEZMO__URI     | https://logs.logdna.com          | Custom ingestion endpoint, optional             |

### Metadata

The library supports recording metadata for the `Microsoft.Extensions.Logging` based logger. This is done using the scope system, see the `Example.Hosting` project for a hands on example. The scope state data will be serialized using `System.Text.Json.JsonSerializer`, you can pass a `string` directly to `ILogger.BeginScope` if you want to fully control the metadata formatting sent to Mezmo.

```
class MyData {
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
}

using (logger.BeginScope(myData)) {
    logger.LogInformation("Processing user request");
}
```

## Contributing

Any pull requests or bug reports are welcome, please try and keep to the existing style conventions and comment any additions.
