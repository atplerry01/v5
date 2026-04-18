using Whycespace.Domain.EconomicSystem.Transaction.Settlement;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Settlement;

/// <summary>
/// Phase 6 T6.4 — drives <see cref="SettlementAggregate"/> from Pending
/// to finality Confirmed when the external provider callback arrives.
/// Reuses <c>MarkCompleted</c> so the aggregate lifecycle stays single-
/// sourced: Completed is the internal encoding of external finality.
///
/// Separate from <see cref="CompleteSettlementHandler"/> so the provider-
/// callback path is distinguishable in audit trails and policy bindings
/// from user-originated completion calls, even though both converge on
/// the same state transition.
/// </summary>
public sealed class ConfirmSettlementFinalityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ConfirmSettlementFinalityCommand cmd)
            return;

        EnforcementGuard.RequireNotRestricted(context.EnforcementConstraint, context.IsSystem);

        var aggregate = (SettlementAggregate)await context.LoadAggregateAsync(typeof(SettlementAggregate));

        if (aggregate.Status == SettlementStatus.Initiated)
            aggregate.MarkProcessing();

        aggregate.MarkCompleted(SettlementReferenceId.From(cmd.ExternalReferenceId));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
