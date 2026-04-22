using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyDefinition;

public sealed class DeprecatePolicyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecatePolicyCommand)
            return;

        var aggregate = (PolicyDefinitionAggregate)await context.LoadAggregateAsync(typeof(PolicyDefinitionAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
