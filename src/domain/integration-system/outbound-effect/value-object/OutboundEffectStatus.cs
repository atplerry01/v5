namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — canonical outbound-effect lifecycle status. Enum integers are
/// APPEND-ONLY; values must be preserved across releases to protect stored
/// event-store integrity (mirrors <c>WorkflowExecutionStatus</c> discipline).
///
/// <para>Terminal states: <see cref="Finalized"/>, <see cref="Cancelled"/>,
/// <see cref="RetryExhausted"/> (terminal unless compensation transitions,
/// R3.B.5), <see cref="Reconciled"/>.</para>
///
/// <para>Transient intermediate states: <see cref="TransientFailed"/> (returns
/// to <see cref="Scheduled"/> on retry), <see cref="CompensationRequested"/>
/// (companion state during compensation workflow, R3.B.5).</para>
/// </summary>
public enum OutboundEffectStatus
{
    Scheduled = 0,
    Dispatched = 1,
    Acknowledged = 2,
    Finalized = 3,
    Cancelled = 4,
    TransientFailed = 5,
    RetryExhausted = 6,
    ReconciliationRequired = 7,
    Reconciled = 8,
    CompensationRequested = 9,
}
