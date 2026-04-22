using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public static class ContractErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("Contract has already been initialized.");

    public static DomainException ContractNameMissing() =>
        new DomainInvariantViolationException("Contract requires a non-empty ContractName.");

    public static DomainException PublisherRouteMissing() =>
        new DomainInvariantViolationException("Contract requires a valid PublisherRoute.");

    public static DomainException SchemaRefMissing() =>
        new DomainInvariantViolationException("Contract requires a non-empty SchemaRef.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("Contract is deprecated and cannot be modified.");
}
