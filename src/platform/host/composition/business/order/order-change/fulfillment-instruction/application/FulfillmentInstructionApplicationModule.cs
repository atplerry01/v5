using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderChange.FulfillmentInstruction.Application;

public static class FulfillmentInstructionApplicationModule
{
    public static IServiceCollection AddFulfillmentInstructionApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateFulfillmentInstructionHandler>();
        services.AddTransient<IssueFulfillmentInstructionHandler>();
        services.AddTransient<CompleteFulfillmentInstructionHandler>();
        services.AddTransient<RevokeFulfillmentInstructionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateFulfillmentInstructionCommand, CreateFulfillmentInstructionHandler>();
        engine.Register<IssueFulfillmentInstructionCommand, IssueFulfillmentInstructionHandler>();
        engine.Register<CompleteFulfillmentInstructionCommand, CompleteFulfillmentInstructionHandler>();
        engine.Register<RevokeFulfillmentInstructionCommand, RevokeFulfillmentInstructionHandler>();
    }
}
