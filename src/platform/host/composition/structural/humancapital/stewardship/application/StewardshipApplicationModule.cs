using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Stewardship;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Stewardship.Application;

public static class StewardshipApplicationModule
{
    public static IServiceCollection AddStewardshipApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateStewardshipHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateStewardshipCommand, CreateStewardshipHandler>();
    }
}
