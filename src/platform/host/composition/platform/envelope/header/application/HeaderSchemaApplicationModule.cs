using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Envelope.Header;
using Whycespace.Shared.Contracts.Platform.Envelope.Header;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Envelope.Header.Application;

public static class HeaderSchemaApplicationModule
{
    public static IServiceCollection AddHeaderSchemaApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterHeaderSchemaHandler>();
        services.AddTransient<DeprecateHeaderSchemaHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterHeaderSchemaCommand, RegisterHeaderSchemaHandler>();
        engine.Register<DeprecateHeaderSchemaCommand, DeprecateHeaderSchemaHandler>();
    }
}
