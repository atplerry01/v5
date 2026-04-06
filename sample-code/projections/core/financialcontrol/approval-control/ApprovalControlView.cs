namespace Whycespace.Projections.Core.Financialcontrol.ApprovalControl;

public sealed record ApprovalControlView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
