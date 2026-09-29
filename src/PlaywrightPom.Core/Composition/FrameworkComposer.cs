using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlaywrightPom.Core.Configuration;
using PlaywrightPom.Core.Logging;
using Serilog;

namespace PlaywrightPom.Core.Composition;

public static class FrameworkComposer
{
    public static ServiceProvider Build(TestSettings settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddSingleton(settings.Run);
        services.AddSingleton(settings.Web);
        services.AddSingleton(settings.Api);
        services.AddLogging(builder => builder.AddSerilog(FrameworkLog.Logger, dispose: false));
        return services.BuildServiceProvider();
    }
}
