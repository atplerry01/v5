namespace Whycespace.Projections.Business.Integration.Secret;

public sealed record SecretView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
