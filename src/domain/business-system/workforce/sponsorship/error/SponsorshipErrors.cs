using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Sponsorship;

public static class SponsorshipErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Sponsorship has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Sponsorship Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Sponsorship Descriptor must not be empty.");
}
