namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public sealed class CanConfirmWriteoffSpecification
{
    public bool IsSatisfiedBy(WriteoffStatus status)
    {
        return status == WriteoffStatus.Pending;
    }
}
