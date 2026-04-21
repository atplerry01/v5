using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Assignment;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Assignment.Application;

public static class AssignmentApplicationModule
{
    public static IServiceCollection AddAssignmentApplication(this IServiceCollection services)
    {
        services.AddTransient<AssignAssignmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<AssignAssignmentCommand, AssignAssignmentHandler>();
    }
}
