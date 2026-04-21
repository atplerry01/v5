namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;

public sealed record ObligationReadModel
{
    public Guid ObligationId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
