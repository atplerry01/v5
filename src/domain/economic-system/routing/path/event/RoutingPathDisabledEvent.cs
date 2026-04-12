using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed record RoutingPathDisabledEvent(
    PathId PathId,
    Timestamp DisabledAt) : DomainEvent;
