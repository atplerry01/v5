using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public static class CommandRoutingErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("CommandRoutingRule has already been initialized.");

    public static DomainException CommandTypeRefMissing() =>
        new DomainInvariantViolationException("CommandRoutingRule requires a valid CommandTypeRef.");

    public static DomainException HandlerRouteMissing() =>
        new DomainInvariantViolationException("CommandRoutingRule requires a valid HandlerRoute.");

    public static DomainException AlreadyRemoved() =>
        new DomainInvariantViolationException("CommandRoutingRule has already been removed.");
}
