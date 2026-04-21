namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;

public sealed record ApprovalReadModel
{
    public Guid ApprovalId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
