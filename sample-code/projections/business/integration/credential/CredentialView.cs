namespace Whycespace.Projections.Business.Integration.Credential;

public sealed record CredentialView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
