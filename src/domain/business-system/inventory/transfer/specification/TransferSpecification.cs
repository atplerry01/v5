namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public sealed class TransferSpecification
{
    public bool IsSatisfiedBy(TransferAggregate transfer)
    {
        return transfer.Id != default
            && transfer.SourceWarehouseId != Guid.Empty
            && transfer.DestinationWarehouseId != Guid.Empty
            && transfer.SourceWarehouseId != transfer.DestinationWarehouseId
            && transfer.Quantity.Value > 0
            && Enum.IsDefined(transfer.Status);
    }
}
