namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B — canonical outcome classification for operator DLQ re-drive.
/// Explicit refusal codes let the admin controller translate each case to a
/// distinct HTTP status without stringly-typed error matching.
/// </summary>
public enum DeadLetterRedriveOutcome
{
    /// <summary>The entry was re-published and marked reprocessed.</summary>
    Accepted,
    /// <summary>No DLQ entry exists for the supplied id.</summary>
    NotFound,
    /// <summary>Entry already marked reprocessed — re-drive is a one-shot operation.</summary>
    AlreadyReprocessed,
    /// <summary>Entry is missing the source topic or payload required for re-publication.</summary>
    Ineligible,
    /// <summary>The underlying publisher failed to deliver the payload.</summary>
    PublishFailed,
}

public sealed record DeadLetterRedriveResult
{
    public required DeadLetterRedriveOutcome Outcome { get; init; }
    public required Guid EventId { get; init; }
    public string? SourceTopic { get; init; }
    public string? FailureReason { get; init; }
    public Guid? AuditEventId { get; init; }

    public bool IsSuccess => Outcome == DeadLetterRedriveOutcome.Accepted;
}
