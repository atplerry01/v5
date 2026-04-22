using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;

public static class RouteDescriptorErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("RouteDescriptor has already been initialized.");

    public static DomainException InvalidSourceRoute() =>
        new DomainInvariantViolationException("RouteDescriptor requires a valid Source DomainRoute.");

    public static DomainException InvalidDestinationRoute() =>
        new DomainInvariantViolationException("RouteDescriptor requires a valid Destination DomainRoute.");

    public static DomainException SelfRoutingForbidden() =>
        new DomainInvariantViolationException("RouteDescriptor Source must not equal Destination.");

    public static DomainException TransportHintMissing() =>
        new DomainInvariantViolationException("RouteDescriptor requires a non-empty TransportHint.");
}
