namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class IsModifiableSpecification
{
    public bool IsSatisfiedBy(ContractDocumentStatus status)
    {
        return status == ContractDocumentStatus.Draft;
    }
}
