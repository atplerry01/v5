namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(FulfillmentInstructionStatus status)
        => status is FulfillmentInstructionStatus.Draft or FulfillmentInstructionStatus.Issued;
}
