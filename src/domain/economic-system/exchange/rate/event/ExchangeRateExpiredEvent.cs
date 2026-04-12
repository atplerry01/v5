using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed record ExchangeRateExpiredEvent(
    RateId RateId,
    Timestamp ExpiredAt) : DomainEvent;
