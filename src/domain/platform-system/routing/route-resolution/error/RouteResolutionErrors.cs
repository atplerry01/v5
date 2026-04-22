using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public static class RouteResolutionErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("RouteResolution has already been initialized.");

    public static DomainException SourceRouteMissing() =>
        new DomainInvariantViolationException("RouteResolution requires a valid SourceRoute.");

    public static DomainException MessageTypeMissing() =>
        new DomainInvariantViolationException("RouteResolution requires a non-empty MessageType.");

    public static DomainException DispatchRulesEmpty() =>
        new DomainInvariantViolationException("RouteResolution requires at least one DispatchRule evaluated.");
}
