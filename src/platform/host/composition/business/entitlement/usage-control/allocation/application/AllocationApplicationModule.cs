using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.UsageControl.Allocation.Application;

public static class AllocationApplicationModule
{
    public static IServiceCollection AddAllocationApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAllocationHandler>();
        services.AddTransient<AllocateAllocationHandler>();
        services.AddTransient<ReleaseAllocationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAllocationCommand, CreateAllocationHandler>();
        engine.Register<AllocateAllocationCommand, AllocateAllocationHandler>();
        engine.Register<ReleaseAllocationCommand, ReleaseAllocationHandler>();
    }
}
