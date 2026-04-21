namespace Whycespace.Shared.Contracts.Structural.Humancapital.Stewardship;

public sealed record StewardshipReadModel
{
    public Guid StewardshipId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
