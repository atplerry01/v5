using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Structure.Classification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Platform.Host.Composition.Structural.Structure.Classification.Application;

public static class ClassificationApplicationModule
{
    public static IServiceCollection AddClassificationApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineClassificationHandler>();
        services.AddTransient<ActivateClassificationHandler>();
        services.AddTransient<DeprecateClassificationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineClassificationCommand, DefineClassificationHandler>();
        engine.Register<ActivateClassificationCommand, ActivateClassificationHandler>();
        engine.Register<DeprecateClassificationCommand, DeprecateClassificationHandler>();
    }
}
