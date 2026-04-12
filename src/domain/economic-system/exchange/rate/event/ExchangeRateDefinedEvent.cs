using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed record ExchangeRateDefinedEvent(
    RateId RateId,
    Currency BaseCurrency,
    Currency QuoteCurrency,
    decimal RateValue,
    Timestamp EffectiveAt,
    int Version) : DomainEvent;
