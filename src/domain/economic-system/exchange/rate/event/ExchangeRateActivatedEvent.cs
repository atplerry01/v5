using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed record ExchangeRateActivatedEvent(
    RateId RateId,
    Timestamp ActivatedAt) : DomainEvent;
