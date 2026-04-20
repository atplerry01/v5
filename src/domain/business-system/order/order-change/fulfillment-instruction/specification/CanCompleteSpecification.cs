namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(FulfillmentInstructionStatus status) => status == FulfillmentInstructionStatus.Issued;
}
