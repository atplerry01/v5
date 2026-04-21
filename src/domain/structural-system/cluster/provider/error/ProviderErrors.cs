using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public static class ProviderErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ProviderId is required and must not be empty.");

    public static DomainException MissingProfile()
        => new DomainInvariantViolationException("ProviderProfile is required.");

    public static DomainException InvalidStateTransition(ProviderStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException InvalidParent()
        => new DomainInvariantViolationException("Provider parent cluster is not in an active state.");

    public static DomainException InvalidAuthorityBinding()
        => new DomainInvariantViolationException("Provider category is not permitted under the supplied authority role.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Provider has already been initialized.");
}
