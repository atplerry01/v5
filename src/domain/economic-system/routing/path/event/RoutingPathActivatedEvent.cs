using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed record RoutingPathActivatedEvent(
    PathId PathId,
    Timestamp ActivatedAt) : DomainEvent;
