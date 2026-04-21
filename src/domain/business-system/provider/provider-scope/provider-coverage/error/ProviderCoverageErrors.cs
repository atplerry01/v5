using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public static class ProviderCoverageErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderCoverageId is required and must not be empty.");

    public static DomainException MissingProviderRef()
        => new DomainInvariantViolationException("ProviderCoverage must reference a provider.");

    public static DomainException InvalidStateTransition(ProviderCoverageStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ProviderCoverageId id)
        => new DomainInvariantViolationException($"ProviderCoverage '{id.Value}' is archived and cannot be mutated.");

    public static DomainException ScopeAlreadyPresent(CoverageScope scope)
        => new DomainInvariantViolationException($"ProviderCoverage already contains scope '{scope.Kind}:{scope.Descriptor}'.");

    public static DomainException ScopeNotPresent(CoverageScope scope)
        => new DomainInvariantViolationException($"ProviderCoverage does not contain scope '{scope.Kind}:{scope.Descriptor}'.");

    public static DomainException ActivationRequiresScope()
        => new DomainInvariantViolationException("ProviderCoverage requires at least one scope before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ProviderCoverage has already been initialized.");
}
