using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

namespace Whycespace.Platform.Host.Composition.Structural.Structure.HierarchyDefinition.Application;

public static class HierarchyDefinitionApplicationModule
{
    public static IServiceCollection AddHierarchyDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineHierarchyDefinitionHandler>();
        services.AddTransient<ValidateHierarchyDefinitionHandler>();
        services.AddTransient<LockHierarchyDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineHierarchyDefinitionCommand, DefineHierarchyDefinitionHandler>();
        engine.Register<ValidateHierarchyDefinitionCommand, ValidateHierarchyDefinitionHandler>();
        engine.Register<LockHierarchyDefinitionCommand, LockHierarchyDefinitionHandler>();
    }
}
