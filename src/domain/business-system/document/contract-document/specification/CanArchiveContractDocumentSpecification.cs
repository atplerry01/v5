namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class CanArchiveContractDocumentSpecification
{
    public bool IsSatisfiedBy(ContractDocumentStatus status)
    {
        return status == ContractDocumentStatus.Finalized;
    }
}
