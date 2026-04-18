using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Restriction.Application;

public static class EnforcementRestrictionApplicationModule
{
    public static IServiceCollection AddEnforcementRestrictionApplication(this IServiceCollection services)
    {
        services.AddTransient<ApplyRestrictionHandler>();
        services.AddTransient<UpdateRestrictionHandler>();
        services.AddTransient<RemoveRestrictionHandler>();
        // Phase 7 T7.7 — suspend/resume lifecycle handlers.
        services.AddTransient<SuspendRestrictionHandler>();
        services.AddTransient<ResumeRestrictionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ApplyRestrictionCommand, ApplyRestrictionHandler>();
        engine.Register<UpdateRestrictionCommand, UpdateRestrictionHandler>();
        engine.Register<RemoveRestrictionCommand, RemoveRestrictionHandler>();
        engine.Register<SuspendRestrictionCommand, SuspendRestrictionHandler>();
        engine.Register<ResumeRestrictionCommand, ResumeRestrictionHandler>();
    }
}
