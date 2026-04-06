namespace Whycespace.Platform.Api.Economic.Capital.Vault;

public sealed record VaultRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VaultResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
