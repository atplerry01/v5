namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(ContractDocumentStatus status)
    {
        return status == ContractDocumentStatus.Draft;
    }
}
