namespace Whycespace.Shared.Contracts.Events.Economic.Exchange.Rate;

public sealed record ExchangeRateDefinedEventSchema(
    Guid AggregateId,
    string BaseCurrency,
    string QuoteCurrency,
    decimal RateValue,
    DateTimeOffset EffectiveAt,
    int Version);

public sealed record ExchangeRateActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record ExchangeRateExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);
