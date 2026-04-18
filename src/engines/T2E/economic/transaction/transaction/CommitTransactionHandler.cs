using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public sealed class CommitTransactionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CommitTransactionCommand cmd)
            return;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var aggregate = (TransactionAggregate)await context.LoadAggregateAsync(typeof(TransactionAggregate));

        var committedAt = new Timestamp(cmd.CommittedAt);

        // Lifecycle gate: Initiated → Processing → Committed. The domain
        // forbids a direct Initiated → Committed transition; compose the
        // two-step transition here when the aggregate has not yet been
        // promoted to Processing, reusing the commit timestamp as the
        // processing-start timestamp (deterministic, no clock read).
        if (aggregate.Status == TransactionStatus.Initiated)
            aggregate.MarkProcessing(committedAt);

        aggregate.Commit(committedAt);

        context.EmitEvents(aggregate.DomainEvents);
    }
}
