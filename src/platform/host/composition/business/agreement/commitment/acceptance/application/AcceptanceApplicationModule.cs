using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Acceptance.Application;

public static class AcceptanceApplicationModule
{
    public static IServiceCollection AddAcceptanceApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAcceptanceHandler>();
        services.AddTransient<AcceptAcceptanceHandler>();
        services.AddTransient<RejectAcceptanceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAcceptanceCommand, CreateAcceptanceHandler>();
        engine.Register<AcceptAcceptanceCommand, AcceptAcceptanceHandler>();
        engine.Register<RejectAcceptanceCommand, RejectAcceptanceHandler>();
    }
}
