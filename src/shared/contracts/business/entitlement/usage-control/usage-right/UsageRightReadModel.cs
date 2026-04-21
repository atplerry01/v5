namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightReadModel
{
    public Guid UsageRightId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid ReferenceId { get; init; }
    public int TotalUnits { get; init; }
    public int TotalUsed { get; init; }
    public int Remaining => TotalUnits - TotalUsed;
    public Guid LastRecordId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
