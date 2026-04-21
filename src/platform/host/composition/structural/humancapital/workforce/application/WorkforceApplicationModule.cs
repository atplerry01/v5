using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Workforce;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Workforce.Application;

public static class WorkforceApplicationModule
{
    public static IServiceCollection AddWorkforceApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateWorkforceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateWorkforceCommand, CreateWorkforceHandler>();
    }
}
