namespace Whycespace.Platform.Api.Business.Integration.Secret;

public sealed record SecretRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SecretResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
