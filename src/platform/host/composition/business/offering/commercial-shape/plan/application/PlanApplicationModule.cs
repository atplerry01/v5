using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Offering.CommercialShape.Plan.Application;

public static class PlanApplicationModule
{
    public static IServiceCollection AddPlanApplication(this IServiceCollection services)
    {
        services.AddTransient<DraftPlanHandler>();
        services.AddTransient<ActivatePlanHandler>();
        services.AddTransient<DeprecatePlanHandler>();
        services.AddTransient<ArchivePlanHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DraftPlanCommand, DraftPlanHandler>();
        engine.Register<ActivatePlanCommand, ActivatePlanHandler>();
        engine.Register<DeprecatePlanCommand, DeprecatePlanHandler>();
        engine.Register<ArchivePlanCommand, ArchivePlanHandler>();
    }
}
