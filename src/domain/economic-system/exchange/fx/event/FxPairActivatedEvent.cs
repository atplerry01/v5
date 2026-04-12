using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed record FxPairActivatedEvent(
    FxId FxId,
    Timestamp ActivatedAt) : DomainEvent;
