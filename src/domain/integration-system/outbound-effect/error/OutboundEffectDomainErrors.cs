namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — domain-layer error constants for outbound-effect lifecycle guard
/// failures. Separate from <c>Whycespace.Shared.Contracts.Runtime.OutboundEffects.OutboundEffectErrors</c>
/// (which carries contract-preconditions + dispatcher errors) because domain
/// purity forbids cross-namespace copy-paste.
/// </summary>
public static class OutboundEffectDomainErrors
{
    public const string ProviderIdRequired = "OutboundEffect ProviderId is required.";
    public const string EffectTypeRequired = "OutboundEffect EffectType is required.";
    public const string IdempotencyKeyRequired = "OutboundEffect IdempotencyKey is required.";
    public const string SchedulerActorIdRequired = "OutboundEffect SchedulerActorId is required.";

    public const string CannotDispatchUnlessScheduled =
        "OutboundEffect can only transition to Dispatched from Scheduled or TransientFailed.";
    public const string CannotAcknowledgeUnlessDispatched =
        "OutboundEffect can only transition to Acknowledged from Dispatched.";
    public const string CannotFinalizeFromTerminal =
        "OutboundEffect cannot finalize from a terminal state.";
    public const string CannotCancelUnlessScheduled =
        "OutboundEffect can only be cancelled from the Scheduled state.";
    public const string AcknowledgedRequiresProviderOperationId =
        "OutboundEffectAcknowledgedEvent requires a non-null ProviderOperationId (R-OUT-EFF-FINALITY-02).";
    public const string CannotRetryAfterExhausted =
        "OutboundEffect cannot record a retry attempt once RetryExhausted.";
    public const string CannotReconcileUnlessRequested =
        "OutboundEffect can only transition to Reconciled from ReconciliationRequired.";
}
