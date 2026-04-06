namespace Whycespace.Projections.Economic.Capital.Vault;

public sealed record VaultView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
