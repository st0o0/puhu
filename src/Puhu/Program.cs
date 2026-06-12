using Microsoft.Extensions.Hosting;
using Servus.Application.Startup;
using Puhu.Setup;

var runner = AppBuilder.Create(Host.CreateApplicationBuilder(args), b => b.Build())
    .WithSetup<LoggingSetup>()
    .WithSetup<ServicesSetup>()
    .WithSetup<MarketplaceSetup>()
    .WithSetup<PluginSetup>()
    .WithSetup<AkkaSetup>()
    .WithSetup<TerminaSetup>()
    .Build();

await runner.RunAsync();
