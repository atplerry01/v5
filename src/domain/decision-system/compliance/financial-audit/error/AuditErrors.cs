namespace Whycespace.Domain.DecisionSystem.Compliance.Audit;

public static class AuditErrors
{
    public const string MissingSourceReference = "Audit record must reference a source domain, aggregate, and event.";
    public const string MissingEvidenceSummary = "Audit record must include an evidence summary.";
    public const string InvalidAuditType = "Audit type is not valid.";
    public const string InvalidStateTransition = "Audit record state transition is not valid.";
    public const string AlreadyFinalized = "Cannot modify a finalized audit record.";
}