namespace Whycespace.Platform.Api.Business.Localization.CurrencyFormat;

public sealed record CurrencyFormatRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CurrencyFormatResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
