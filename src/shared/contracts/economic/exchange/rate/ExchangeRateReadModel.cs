namespace Whycespace.Shared.Contracts.Economic.Exchange.Rate;

public sealed record ExchangeRateReadModel
{
    public Guid RateId { get; init; }
    public string BaseCurrency { get; init; } = string.Empty;
    public string QuoteCurrency { get; init; } = string.Empty;
    public decimal RateValue { get; init; }
    public DateTimeOffset EffectiveAt { get; init; }
    public int VersionNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
