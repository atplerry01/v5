using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Sanction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Sanction.Application;

public static class SanctionApplicationModule
{
    public static IServiceCollection AddSanctionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSanctionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSanctionCommand, CreateSanctionHandler>();
    }
}
