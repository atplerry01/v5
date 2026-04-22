using Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationState;

public sealed class SetConfigurationStateHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SetConfigurationStateCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationStateAggregate.Set(
            new ConfigurationStateId(cmd.StateId.ToString("N").PadRight(64, '0')),
            cmd.DefinitionId,
            cmd.Value,
            cmd.Version);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
