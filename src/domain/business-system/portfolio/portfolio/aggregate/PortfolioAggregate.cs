namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed class PortfolioAggregate
{
    public static PortfolioAggregate Create()
    {
        var aggregate = new PortfolioAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
