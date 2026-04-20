namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public sealed class CanFulfillSpecification
{
    public bool IsSatisfiedBy(ObligationStatus status)
    {
        return status == ObligationStatus.Pending;
    }
}
