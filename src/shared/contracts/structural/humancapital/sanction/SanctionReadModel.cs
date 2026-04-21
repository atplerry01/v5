namespace Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

public sealed record SanctionReadModel
{
    public Guid SanctionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
