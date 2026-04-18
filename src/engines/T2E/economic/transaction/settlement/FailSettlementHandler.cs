using Whycespace.Domain.EconomicSystem.Transaction.Settlement;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Settlement;

public sealed class FailSettlementHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailSettlementCommand cmd)
            return;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var aggregate = (SettlementAggregate)await context.LoadAggregateAsync(typeof(SettlementAggregate));

        if (aggregate.Status == SettlementStatus.Initiated)
            aggregate.MarkProcessing();

        aggregate.MarkFailed(cmd.Reason ?? string.Empty);

        context.EmitEvents(aggregate.DomainEvents);
    }
}
