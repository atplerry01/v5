namespace Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

public sealed record ReputationReadModel
{
    public Guid ReputationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
