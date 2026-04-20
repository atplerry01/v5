namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public sealed class CanIssueSpecification
{
    public bool IsSatisfiedBy(FulfillmentInstructionStatus status) => status == FulfillmentInstructionStatus.Draft;
}
