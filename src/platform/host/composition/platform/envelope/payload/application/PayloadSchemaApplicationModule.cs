using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Envelope.Payload;
using Whycespace.Shared.Contracts.Platform.Envelope.Payload;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Envelope.Payload.Application;

public static class PayloadSchemaApplicationModule
{
    public static IServiceCollection AddPayloadSchemaApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterPayloadSchemaHandler>();
        services.AddTransient<DeprecatePayloadSchemaHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterPayloadSchemaCommand, RegisterPayloadSchemaHandler>();
        engine.Register<DeprecatePayloadSchemaCommand, DeprecatePayloadSchemaHandler>();
    }
}
