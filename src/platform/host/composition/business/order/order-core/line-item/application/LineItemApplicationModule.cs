using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderCore.LineItem.Application;

public static class LineItemApplicationModule
{
    public static IServiceCollection AddLineItemApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateLineItemHandler>();
        services.AddTransient<UpdateLineItemQuantityHandler>();
        services.AddTransient<CancelLineItemHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateLineItemCommand, CreateLineItemHandler>();
        engine.Register<UpdateLineItemQuantityCommand, UpdateLineItemQuantityHandler>();
        engine.Register<CancelLineItemCommand, CancelLineItemHandler>();
    }
}
