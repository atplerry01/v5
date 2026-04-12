namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PortfolioStatus status)
    {
        return status == PortfolioStatus.Draft;
    }
}
