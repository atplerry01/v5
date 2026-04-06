namespace Whycespace.Projections.Business.Localization.CurrencyFormat;

public sealed record CurrencyFormatView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
