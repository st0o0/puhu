using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Servus.Application.Startup;

namespace Servus.TUI.Setup;

public sealed class LoggingSetup : IServiceSetupContainer, ILoggingSetupContainer
{
    public void SetupLogging(ILoggingBuilder builder)
    {
        builder.ClearProviders();
    }

    public void SetupServices(IServiceCollection services, IConfiguration configuration)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".servus", "logs", "servus-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        services.AddSerilog();
    }
}
