namespace Whycespace.Platform.Api.Economic.Transaction.Wallet;

public sealed record WalletRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WalletResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
