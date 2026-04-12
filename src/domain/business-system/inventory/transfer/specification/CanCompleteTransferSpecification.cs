namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public sealed class CanCompleteTransferSpecification
{
    public bool IsSatisfiedBy(TransferStatus status)
    {
        return status == TransferStatus.Pending;
    }
}
