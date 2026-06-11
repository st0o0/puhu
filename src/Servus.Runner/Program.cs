using Microsoft.Extensions.Hosting;
using Servus.Application.Startup;
using Servus.Runner.Setup;

var runner = AppBuilder.Create(Host.CreateApplicationBuilder(args), b => b.Build())
    .WithSetup<LoggingSetup>()
    .WithSetup<ServicesSetup>()
    .WithSetup<MarketplaceSetup>()
    .WithSetup<PluginSetup>()
    .WithSetup<ActorSystemSetup>()
    .WithSetup<TerminaSetup>()
    .Build();

await runner.RunAsync();
