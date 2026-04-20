namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.5 — pure policy for deciding whether a given (outcome, shape) pair
/// requires automatic <see cref="OutboundEffectCompensationRequestedEvent"/>
/// emission. Replay-safe (no wall-clock or random inputs).
///
/// <para><b>Rules:</b>
/// <list type="bullet">
///   <item><c>BusinessFailed</c> — ALWAYS emits compensation (irrespective of shape).
///         A partner rejected the operation; local preparatory state may need to unwind.</item>
///   <item><c>PartiallyCompleted</c> — emits for <c>AtMostOnceRequired</c> and
///         <c>CompensatableOnly</c> shapes, where the provider's partial acceptance
///         cannot be recovered by re-send. For <c>ProviderIdempotent</c> /
///         <c>NaturalKeyIdempotent</c>, the caller can re-schedule the missing
///         segment idempotently and does not need compensation.</item>
///   <item><c>ManualIntervention</c> — does NOT auto-emit; the operator-driven
///         reconcile path surfaces the decision separately.</item>
///   <item><c>Succeeded</c> — never emits.</item>
/// </list></para>
/// </summary>
public static class OutboundEffectCompensationPolicy
{
    public static bool RequiresCompensation(
        OutboundFinalityOutcome outcome,
        OutboundIdempotencyShape shape)
        => outcome switch
        {
            OutboundFinalityOutcome.BusinessFailed => true,
            OutboundFinalityOutcome.PartiallyCompleted =>
                shape is OutboundIdempotencyShape.AtMostOnceRequired
                    or OutboundIdempotencyShape.CompensatableOnly,
            _ => false,
        };
}
