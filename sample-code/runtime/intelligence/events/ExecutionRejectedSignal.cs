namespace Whycespace.Runtime.Intelligence.Events;

/// <summary>
/// Runtime signal emitted when execution governance validation fails (E18.4).
/// Consumed by audit trail and alerting.
/// </summary>
public sealed record ExecutionRejectedSignal(
    Guid RootEntityId,
    string Reason);
