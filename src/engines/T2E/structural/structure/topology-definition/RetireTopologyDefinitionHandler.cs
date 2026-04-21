using Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.TopologyDefinition;

public sealed class RetireTopologyDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireTopologyDefinitionCommand) return;
        var aggregate = (TopologyDefinitionAggregate)await context.LoadAggregateAsync(typeof(TopologyDefinitionAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
