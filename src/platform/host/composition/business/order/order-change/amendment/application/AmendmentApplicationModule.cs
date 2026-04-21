using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderChange.Amendment.Application;

public static class AmendmentApplicationModule
{
    public static IServiceCollection AddOrderChangeAmendmentApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestAmendmentHandler>();
        services.AddTransient<AcceptAmendmentHandler>();
        services.AddTransient<ApplyAmendmentHandler>();
        services.AddTransient<RejectAmendmentHandler>();
        services.AddTransient<CancelAmendmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestAmendmentCommand, RequestAmendmentHandler>();
        engine.Register<AcceptAmendmentCommand, AcceptAmendmentHandler>();
        engine.Register<ApplyAmendmentCommand, ApplyAmendmentHandler>();
        engine.Register<RejectAmendmentCommand, RejectAmendmentHandler>();
        engine.Register<CancelAmendmentCommand, CancelAmendmentHandler>();
    }
}
