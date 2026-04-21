namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;

public sealed record AmendmentReadModel
{
    public Guid AmendmentId { get; init; }
    public Guid TargetId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
