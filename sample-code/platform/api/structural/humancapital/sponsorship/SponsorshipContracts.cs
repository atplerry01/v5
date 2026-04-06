namespace Whycespace.Platform.Api.Structural.Humancapital.Sponsorship;

public sealed record SponsorshipRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SponsorshipResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
