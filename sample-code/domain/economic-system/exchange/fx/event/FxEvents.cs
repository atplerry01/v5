using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed record FxRatePublishedEvent(Guid RateId, string BaseCurrency, string QuoteCurrency, decimal Rate, DateTimeOffset EffectiveFrom, string Source) : DomainEvent;
public sealed record FxRateSupersededEvent(Guid RateId, DateTimeOffset EffectiveUntil) : DomainEvent;
public sealed record FxRateInvalidatedEvent(Guid RateId, string Reason) : DomainEvent;
