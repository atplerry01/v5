namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(PortfolioStatus status)
    {
        return status == PortfolioStatus.Closed;
    }
}
