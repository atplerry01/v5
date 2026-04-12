using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed record FxPairRegisteredEvent(
    FxId FxId,
    CurrencyPair CurrencyPair) : DomainEvent;
