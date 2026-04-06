namespace Whycespace.Runtime.Intelligence.Events;

/// <summary>
/// Runtime signal emitted when execution governance validation passes (E18.4).
/// Consumed by WSS orchestrator and audit trail.
/// </summary>
public sealed record ExecutionApprovedSignal(
    Guid RootEntityId,
    IReadOnlyCollection<Guid> Path);
