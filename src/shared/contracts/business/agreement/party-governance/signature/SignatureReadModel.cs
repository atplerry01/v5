namespace Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;

public sealed record SignatureReadModel
{
    public Guid SignatureId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
