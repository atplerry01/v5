namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public sealed class WriteoffSpecification
{
    public bool IsSatisfiedBy(WriteoffAggregate writeoff)
    {
        return writeoff.Id != default
            && writeoff.Reference.Value != Guid.Empty
            && writeoff.Quantity.Value > 0
            && !string.IsNullOrWhiteSpace(writeoff.Reason.Value)
            && Enum.IsDefined(writeoff.Status);
    }
}
