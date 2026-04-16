using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public sealed class EvidenceRecord
{
    public Guid EvidenceId { get; }
    public string ReporterRef { get; }
    public string Description { get; }
    public Timestamp RecordedAt { get; }

    private EvidenceRecord(Guid evidenceId, string reporterRef, string description, Timestamp at)
    {
        EvidenceId = evidenceId;
        ReporterRef = reporterRef;
        Description = description;
        RecordedAt = at;
    }

    public static EvidenceRecord Attach(Guid evidenceId, string reporterRef, string description, Timestamp at)
    {
        if (evidenceId == Guid.Empty) throw ModerationErrors.InvalidEvidence();
        if (string.IsNullOrWhiteSpace(reporterRef)) throw ModerationErrors.InvalidEvidence();
        if (string.IsNullOrWhiteSpace(description)) throw ModerationErrors.InvalidEvidence();
        return new EvidenceRecord(evidenceId, reporterRef.Trim(), description.Trim(), at);
    }
}
