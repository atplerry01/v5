using Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.TopologyDefinition;

public sealed class CreateTopologyDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateTopologyDefinitionCommand cmd) return Task.CompletedTask;
        var aggregate = TopologyDefinitionAggregate.Create(
            new TopologyDefinitionId(cmd.TopologyDefinitionId),
            new TopologyDefinitionDescriptor(cmd.DefinitionName, cmd.DefinitionKind));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
