using Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationDefinition;

public sealed class DefineConfigurationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineConfigurationCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationDefinitionAggregate.Define(
            new ConfigurationDefinitionId(cmd.DefinitionId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            Enum.Parse<ConfigValueType>(cmd.ValueType, ignoreCase: true),
            cmd.Description,
            cmd.DefaultValue);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
