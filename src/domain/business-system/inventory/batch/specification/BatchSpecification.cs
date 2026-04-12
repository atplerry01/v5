namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public sealed class BatchSpecification
{
    public bool IsSatisfiedBy(BatchAggregate batch)
    {
        return batch.Id != default
            && Enum.IsDefined(batch.Status);
    }
}
