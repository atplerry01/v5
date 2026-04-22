using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Command.CommandDefinition.Application;

public static class CommandDefinitionApplicationModule
{
    public static IServiceCollection AddCommandDefinitionApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineCommandHandler>();
        services.AddTransient<DeprecateCommandDefinitionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineCommandCommand, DefineCommandHandler>();
        engine.Register<DeprecateCommandDefinitionCommand, DeprecateCommandDefinitionHandler>();
    }
}
