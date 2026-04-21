namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;

public sealed record AcceptanceReadModel
{
    public Guid AcceptanceId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
