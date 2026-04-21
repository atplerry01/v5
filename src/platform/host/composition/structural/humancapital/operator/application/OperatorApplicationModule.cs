using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Operator;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Operator;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Operator.Application;

public static class OperatorApplicationModule
{
    public static IServiceCollection AddOperatorApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateOperatorHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateOperatorCommand, CreateOperatorHandler>();
    }
}
