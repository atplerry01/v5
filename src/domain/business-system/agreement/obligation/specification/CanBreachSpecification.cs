namespace Whycespace.Domain.BusinessSystem.Agreement.Obligation;

public sealed class CanBreachSpecification
{
    public bool IsSatisfiedBy(ObligationStatus status)
    {
        return status == ObligationStatus.Pending;
    }
}
