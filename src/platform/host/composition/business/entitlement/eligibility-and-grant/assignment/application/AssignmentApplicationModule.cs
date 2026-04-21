using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Entitlement.EligibilityAndGrant.Assignment.Application;

public static class AssignmentApplicationModule
{
    public static IServiceCollection AddAssignmentApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAssignmentHandler>();
        services.AddTransient<ActivateAssignmentHandler>();
        services.AddTransient<RevokeAssignmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAssignmentCommand, CreateAssignmentHandler>();
        engine.Register<ActivateAssignmentCommand, ActivateAssignmentHandler>();
        engine.Register<RevokeAssignmentCommand, RevokeAssignmentHandler>();
    }
}
