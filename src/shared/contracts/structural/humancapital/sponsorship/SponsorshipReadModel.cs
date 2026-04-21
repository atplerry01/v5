namespace Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

public sealed record SponsorshipReadModel
{
    public Guid SponsorshipId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
