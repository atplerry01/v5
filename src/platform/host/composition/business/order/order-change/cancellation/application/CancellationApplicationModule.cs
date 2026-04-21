using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderChange.Cancellation.Application;

public static class CancellationApplicationModule
{
    public static IServiceCollection AddCancellationApplication(this IServiceCollection services)
    {
        services.AddTransient<RequestCancellationHandler>();
        services.AddTransient<ConfirmCancellationHandler>();
        services.AddTransient<RejectCancellationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RequestCancellationCommand, RequestCancellationHandler>();
        engine.Register<ConfirmCancellationCommand, ConfirmCancellationHandler>();
        engine.Register<RejectCancellationCommand, RejectCancellationHandler>();
    }
}
