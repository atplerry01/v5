namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(PortfolioStatus status)
    {
        return status == PortfolioStatus.Active;
    }
}
