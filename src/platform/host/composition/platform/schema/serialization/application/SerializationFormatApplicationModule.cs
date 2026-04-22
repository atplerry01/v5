using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Schema.Serialization.Application;

public static class SerializationFormatApplicationModule
{
    public static IServiceCollection AddSerializationFormatApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterSerializationFormatHandler>();
        services.AddTransient<DeprecateSerializationFormatHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterSerializationFormatCommand, RegisterSerializationFormatHandler>();
        engine.Register<DeprecateSerializationFormatCommand, DeprecateSerializationFormatHandler>();
    }
}
