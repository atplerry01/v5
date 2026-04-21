namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;

public sealed record RenewalReadModel
{
    public Guid RenewalId { get; init; }
    public Guid SourceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
