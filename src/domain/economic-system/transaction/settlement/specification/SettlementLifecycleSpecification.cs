using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Encodes the settlement lifecycle as pure transitions:
///   Initiated  -> Processing
///   Processing -> Completed  (terminal)
///   Processing -> Failed     (terminal)
/// Terminal states are irreversible — no transition out of Completed or Failed
/// is ever allowed. This is the domain-level guarantee that settlement cannot
/// be rewritten once external execution is recorded.
/// </summary>
public sealed class SettlementLifecycleSpecification : Specification<(SettlementStatus From, SettlementStatus To)>
{
    public override bool IsSatisfiedBy((SettlementStatus From, SettlementStatus To) transition) =>
        transition switch
        {
            (SettlementStatus.Initiated, SettlementStatus.Processing) => true,
            (SettlementStatus.Processing, SettlementStatus.Completed) => true,
            (SettlementStatus.Processing, SettlementStatus.Failed) => true,
            _ => false
        };

    public static bool IsTerminal(SettlementStatus status) =>
        status is SettlementStatus.Completed or SettlementStatus.Failed;

    public bool CanProcess(SettlementStatus status) =>
        IsSatisfiedBy((status, SettlementStatus.Processing));

    public bool CanComplete(SettlementStatus status) =>
        IsSatisfiedBy((status, SettlementStatus.Completed));

    public bool CanFail(SettlementStatus status) =>
        IsSatisfiedBy((status, SettlementStatus.Failed));

    public bool PreventReversal(SettlementStatus status) => IsTerminal(status);
}
