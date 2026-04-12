namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public sealed class CanCloseBatchSpecification
{
    public bool IsSatisfiedBy(BatchStatus status)
    {
        return status == BatchStatus.Open;
    }
}
