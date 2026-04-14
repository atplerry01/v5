using Whycespace.Domain.EconomicSystem.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Settlement;

public sealed class CompleteSettlementHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteSettlementCommand cmd)
            return;

        var aggregate = (SettlementAggregate)await context.LoadAggregateAsync(typeof(SettlementAggregate));

        // Natural progression: external rail reports processing then completion
        // in a single domain-level command. If the aggregate has not yet
        // transitioned to Processing, move it there before completing.
        if (aggregate.Status == SettlementStatus.Initiated)
            aggregate.MarkProcessing();

        aggregate.MarkCompleted(SettlementReferenceId.From(cmd.ExternalReferenceId));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
