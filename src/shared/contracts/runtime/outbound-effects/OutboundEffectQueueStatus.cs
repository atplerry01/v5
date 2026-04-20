namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — string constants used by the queue table's <c>status</c> column.
/// Values mirror the aggregate's terminal and in-flight statuses. String-keyed
/// so SQL migrations and read models do not need to round-trip through an
/// enum ordinal that may shift.
/// </summary>
public static class OutboundEffectQueueStatus
{
    public const string Scheduled = "Scheduled";
    public const string Dispatched = "Dispatched";
    public const string Acknowledged = "Acknowledged";
    public const string Finalized = "Finalized";
    public const string TransientFailed = "TransientFailed";
    public const string RetryExhausted = "RetryExhausted";
    public const string ReconciliationRequired = "ReconciliationRequired";
    public const string Reconciled = "Reconciled";
    public const string Cancelled = "Cancelled";
    public const string CompensationRequested = "CompensationRequested";
}
