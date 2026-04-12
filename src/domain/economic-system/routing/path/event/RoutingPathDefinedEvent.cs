using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed record RoutingPathDefinedEvent(
    PathId PathId,
    PathType PathType,
    string Conditions,
    int Priority) : DomainEvent;
