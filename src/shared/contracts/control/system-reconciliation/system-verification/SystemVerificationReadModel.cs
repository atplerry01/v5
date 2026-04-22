namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;

public sealed record SystemVerificationReadModel
{
    public Guid VerificationId { get; init; }
    public string TargetSystem { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public DateTimeOffset? PassedAt { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset? FailedAt { get; init; }
}
