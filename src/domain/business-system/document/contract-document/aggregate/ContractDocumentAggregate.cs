namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class ContractDocumentAggregate
{
    public static ContractDocumentAggregate Create()
    {
        var aggregate = new ContractDocumentAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
