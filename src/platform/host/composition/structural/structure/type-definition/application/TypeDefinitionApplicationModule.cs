using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Structure.TypeDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Platform.Host.Composition.Structural.Structure.TypeDefinition.Application;

public static class TypeDefinitionApplicationModule
{
    public static IServiceCollection AddTypeDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineTypeDefinitionHandler>();
        services.AddTransient<ActivateTypeDefinitionHandler>();
        services.AddTransient<RetireTypeDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineTypeDefinitionCommand, DefineTypeDefinitionHandler>();
        engine.Register<ActivateTypeDefinitionCommand, ActivateTypeDefinitionHandler>();
        engine.Register<RetireTypeDefinitionCommand, RetireTypeDefinitionHandler>();
    }
}
