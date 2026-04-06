namespace Whycespace.Platform.Api.Trust.Identity.Credential;

public sealed record CredentialRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CredentialResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
