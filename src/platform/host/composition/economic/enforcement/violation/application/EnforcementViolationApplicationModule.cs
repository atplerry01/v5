using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Violation.Application;

public static class EnforcementViolationApplicationModule
{
    public static IServiceCollection AddEnforcementViolationApplication(this IServiceCollection services)
    {
        services.AddTransient<DetectViolationHandler>();
        services.AddTransient<AcknowledgeViolationHandler>();
        services.AddTransient<ResolveViolationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DetectViolationCommand, DetectViolationHandler>();
        engine.Register<AcknowledgeViolationCommand, AcknowledgeViolationHandler>();
        engine.Register<ResolveViolationCommand, ResolveViolationHandler>();
    }
}
