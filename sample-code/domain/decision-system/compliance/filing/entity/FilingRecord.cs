namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.SharedKernel;

public sealed class FilingRecord : Entity
{
    public Guid ReportId { get; private set; }
    public string FilingType { get; private set; } = default!;
    public string ReferenceNumber { get; private set; } = default!;
    public string SubmittedBy { get; private set; } = default!;
    public DateTimeOffset FiledAt { get; private set; }

    private FilingRecord() { }

    public static FilingRecord Create(
        Guid filingId,
        Guid reportId,
        string filingType,
        string referenceNumber,
        string submittedBy,
        DateTimeOffset timestamp)
    {
        return new FilingRecord
        {
            Id = filingId,
            ReportId = reportId,
            FilingType = filingType,
            ReferenceNumber = referenceNumber,
            SubmittedBy = submittedBy,
            FiledAt = timestamp
        };
    }
}
