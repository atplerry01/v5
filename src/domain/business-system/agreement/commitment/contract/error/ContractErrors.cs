using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public static class ContractErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ContractId is required and must not be empty.");

    public static DomainException AlreadyActive(ContractId id)
        => new DomainInvariantViolationException($"Contract '{id.Value}' is already active.");

    public static DomainException AlreadyTerminated(ContractId id)
        => new DomainInvariantViolationException($"Contract '{id.Value}' has already been terminated.");

    public static DomainException InvalidStateTransition(ContractStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException PartyRequired()
        => new DomainInvariantViolationException("Contract must have at least one party.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Contract has already been initialized.");
}
