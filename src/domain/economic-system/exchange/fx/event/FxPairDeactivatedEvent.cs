using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed record FxPairDeactivatedEvent(
    FxId FxId,
    Timestamp DeactivatedAt) : DomainEvent;
