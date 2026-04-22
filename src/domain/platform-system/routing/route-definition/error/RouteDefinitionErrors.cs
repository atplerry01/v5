using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public static class RouteDefinitionErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("RouteDefinition has already been initialized.");

    public static DomainException RouteNameMissing() =>
        new DomainInvariantViolationException("RouteDefinition requires a non-empty RouteName.");

    public static DomainException InvalidSourceRoute() =>
        new DomainInvariantViolationException("RouteDefinition requires a valid SourceRoute.");

    public static DomainException InvalidDestinationRoute() =>
        new DomainInvariantViolationException("RouteDefinition requires a valid DestinationRoute.");

    public static DomainException SelfRoutingForbidden() =>
        new DomainInvariantViolationException("RouteDefinition SourceRoute must not equal DestinationRoute.");

    public static DomainException PriorityNegative() =>
        new DomainInvariantViolationException("RouteDefinition Priority must be non-negative.");

    public static DomainException AlreadyInactive() =>
        new DomainInvariantViolationException("RouteDefinition is already inactive.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("RouteDefinition is already deprecated and cannot be modified.");
}
