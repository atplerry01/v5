using Whycespace.Domain.ControlSystem.Enforcement.Rule;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Rule;

public sealed class RetireEnforcementRuleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireEnforcementRuleCommand)
            return;

        var aggregate = (EnforcementRuleAggregate)await context.LoadAggregateAsync(typeof(EnforcementRuleAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
