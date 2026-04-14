using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public sealed class FailTransactionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailTransactionCommand cmd)
            return;

        var aggregate = (TransactionAggregate)await context.LoadAggregateAsync(typeof(TransactionAggregate));

        var failedAt = new Timestamp(cmd.FailedAt);

        // Lifecycle gate: Initiated → Processing → Failed. The domain
        // forbids a direct Initiated → Failed transition; compose the
        // two-step transition here when the aggregate has not yet been
        // promoted to Processing, reusing the failure timestamp as the
        // processing-start timestamp (deterministic, no clock read).
        if (aggregate.Status == TransactionStatus.Initiated)
            aggregate.MarkProcessing(failedAt);

        aggregate.Fail(cmd.Reason, failedAt);

        context.EmitEvents(aggregate.DomainEvents);
    }
}
