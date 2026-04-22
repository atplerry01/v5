using Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationAssignment;

public sealed class AssignConfigurationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssignConfigurationCommand cmd)
            return Task.CompletedTask;

        var aggregate = ConfigurationAssignmentAggregate.Assign(
            new ConfigurationAssignmentId(cmd.AssignmentId.ToString("N").PadRight(64, '0')),
            cmd.DefinitionId,
            cmd.ScopeId,
            cmd.Value);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
