namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;

public sealed record ValidityReadModel
{
    public Guid ValidityId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
