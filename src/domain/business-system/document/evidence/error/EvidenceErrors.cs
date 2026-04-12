namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public static class EvidenceErrors
{
    public static EvidenceDomainException MissingId()
        => new("EvidenceId is required and must not be empty.");

    public static EvidenceDomainException AlreadyVerified(EvidenceId id)
        => new($"Evidence '{id.Value}' has already been verified.");

    public static EvidenceDomainException AlreadyArchived(EvidenceId id)
        => new($"Evidence '{id.Value}' has already been archived.");

    public static EvidenceDomainException InvalidStateTransition(EvidenceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EvidenceDomainException CannotMutateAfterCapture()
        => new("Evidence is immutable after capture. Only verification and archival transitions are permitted.");
}

public sealed class EvidenceDomainException : Exception
{
    public EvidenceDomainException(string message) : base(message) { }
}
