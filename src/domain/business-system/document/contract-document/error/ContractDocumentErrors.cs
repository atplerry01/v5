namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public static class ContractDocumentErrors
{
    public static ContractDocumentDomainException MissingId()
        => new("ContractDocumentId is required and must not be empty.");

    public static ContractDocumentDomainException MissingContractReferenceId()
        => new("ContractReferenceId is required and must not be empty.");

    public static ContractDocumentDomainException AlreadyFinalized(ContractDocumentId id)
        => new($"ContractDocument '{id.Value}' has already been finalized.");

    public static ContractDocumentDomainException AlreadyArchived(ContractDocumentId id)
        => new($"ContractDocument '{id.Value}' has already been archived.");

    public static ContractDocumentDomainException InvalidStateTransition(ContractDocumentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ContractDocumentDomainException CannotModifyAfterFinalization()
        => new("Cannot modify a contract document after finalization.");
}

public sealed class ContractDocumentDomainException : Exception
{
    public ContractDocumentDomainException(string message) : base(message) { }
}
