namespace Whycespace.Projections.Business.Agreement.Approval;

public sealed record ApprovalView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
