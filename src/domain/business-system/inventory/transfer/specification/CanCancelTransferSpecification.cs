namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public sealed class CanCancelTransferSpecification
{
    public bool IsSatisfiedBy(TransferStatus status)
    {
        return status == TransferStatus.Pending;
    }
}
