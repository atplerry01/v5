using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderCore.Order.Application;

public static class OrderApplicationModule
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateOrderHandler>();
        services.AddTransient<ConfirmOrderHandler>();
        services.AddTransient<CompleteOrderHandler>();
        services.AddTransient<CancelOrderHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateOrderCommand, CreateOrderHandler>();
        engine.Register<ConfirmOrderCommand, ConfirmOrderHandler>();
        engine.Register<CompleteOrderCommand, CompleteOrderHandler>();
        engine.Register<CancelOrderCommand, CancelOrderHandler>();
    }
}
