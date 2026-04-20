namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public static class ProviderCoverageErrors
{
    public static ProviderCoverageDomainException MissingId()
        => new("ProviderCoverageId is required and must not be empty.");

    public static ProviderCoverageDomainException MissingProviderRef()
        => new("ProviderCoverage must reference a provider.");

    public static ProviderCoverageDomainException InvalidStateTransition(ProviderCoverageStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProviderCoverageDomainException ArchivedImmutable(ProviderCoverageId id)
        => new($"ProviderCoverage '{id.Value}' is archived and cannot be mutated.");

    public static ProviderCoverageDomainException ScopeAlreadyPresent(CoverageScope scope)
        => new($"ProviderCoverage already contains scope '{scope.Kind}:{scope.Descriptor}'.");

    public static ProviderCoverageDomainException ScopeNotPresent(CoverageScope scope)
        => new($"ProviderCoverage does not contain scope '{scope.Kind}:{scope.Descriptor}'.");

    public static ProviderCoverageDomainException ActivationRequiresScope()
        => new("ProviderCoverage requires at least one scope before activation.");
}

public sealed class ProviderCoverageDomainException : Exception
{
    public ProviderCoverageDomainException(string message) : base(message) { }
}
