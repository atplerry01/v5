namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public static class ContractErrors
{
    public static ContractDomainException MissingId()
        => new("ContractId is required and must not be empty.");

    public static ContractDomainException AlreadyActive(ContractId id)
        => new($"Contract '{id.Value}' is already active.");

    public static ContractDomainException AlreadyTerminated(ContractId id)
        => new($"Contract '{id.Value}' has already been terminated.");

    public static ContractDomainException InvalidStateTransition(ContractStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ContractDomainException PartyRequired()
        => new("Contract must have at least one party.");
}

public sealed class ContractDomainException : Exception
{
    public ContractDomainException(string message) : base(message) { }
}
