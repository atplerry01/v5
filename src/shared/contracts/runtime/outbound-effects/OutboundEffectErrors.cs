namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — canonical error messages for outbound-effect preconditions and
/// contract-level validation. Kept in shared contracts so both the dispatcher
/// and the domain aggregate reference the same string constants.
/// </summary>
public static class OutboundEffectErrors
{
    public const string IdempotencyKeyRequired =
        "OutboundEffectIntent.IdempotencyKey is required (R-OUT-EFF-IDEM-01).";

    public const string IdempotencyKeyTooLong =
        "OutboundEffectIntent.IdempotencyKey exceeds the 255-character cap (R-OUT-EFF-IDEM-01 / D-R3B1-3).";

    public const string ProviderIdRequired =
        "OutboundEffectIntent.ProviderId is required.";

    public const string EffectTypeRequired =
        "OutboundEffectIntent.EffectType is required.";

    public const string PayloadRequired =
        "OutboundEffectIntent.Payload is required.";

    public const string ProviderNotRegistered =
        "No IOutboundEffectAdapter registered for the supplied ProviderId.";

    public const string AcknowledgedRequiresProviderOperationId =
        "OutboundEffectAcknowledgedEvent requires a non-null ProviderOperationIdentity (R-OUT-EFF-FINALITY-02).";

    public const string OptionsMissing =
        "OutboundEffectOptions for the requested ProviderId is not configured (R-OUT-EFF-TIMEOUT-02).";

    public const string CannotCancelUnlessScheduled =
        "OutboundEffectAggregate can only be cancelled from the Scheduled state.";

    public const string CannotDispatchUnlessScheduled =
        "OutboundEffectAggregate can only transition to Dispatched from Scheduled (or TransientFailed on retry).";

    public const string CannotAcknowledgeUnlessDispatched =
        "OutboundEffectAggregate can only transition to Acknowledged from Dispatched.";

    public const string CannotFinalizeFromTerminal =
        "OutboundEffectAggregate cannot finalize once in a terminal state.";

    public const string CannotRetryAfterDispatchForAtMostOnce =
        "AtMostOnceRequired adapters cannot retry after the Dispatched transition (R-OUT-EFF-IDEM-05).";

    public const string PayloadTooLarge =
        "OutboundEffectIntent.Payload exceeds the 256 KB cap (D-R3B1-3).";
}
