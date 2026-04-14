using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Capital.Allocation.Application;

public static class CapitalAllocationApplicationModule
{
    public static IServiceCollection AddCapitalAllocationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCapitalAllocationHandler>();
        services.AddTransient<ReleaseCapitalAllocationHandler>();
        services.AddTransient<CompleteCapitalAllocationHandler>();
        services.AddTransient<AllocateCapitalToSpvHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateCapitalAllocationCommand, CreateCapitalAllocationHandler>();
        engine.Register<ReleaseCapitalAllocationCommand, ReleaseCapitalAllocationHandler>();
        engine.Register<CompleteCapitalAllocationCommand, CompleteCapitalAllocationHandler>();
        engine.Register<AllocateCapitalToSpvCommand, AllocateCapitalToSpvHandler>();
    }
}
