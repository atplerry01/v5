using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Exchange.Rate;

public sealed record DefineExchangeRateCommand(
    Guid RateId,
    string BaseCurrency,
    string QuoteCurrency,
    decimal RateValue,
    DateTimeOffset EffectiveAt,
    int Version) : IHasAggregateId
{
    public Guid AggregateId => RateId;
}

public sealed record ActivateExchangeRateCommand(
    Guid RateId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => RateId;
}

public sealed record ExpireExchangeRateCommand(
    Guid RateId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => RateId;
}
