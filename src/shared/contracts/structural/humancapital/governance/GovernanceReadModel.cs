namespace Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

public sealed record GovernanceReadModel
{
    public Guid GovernanceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
