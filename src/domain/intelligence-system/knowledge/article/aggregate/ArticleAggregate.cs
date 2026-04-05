namespace Whycespace.Domain.IntelligenceSystem.Knowledge.Article;

public sealed class ArticleAggregate
{
    public static ArticleAggregate Create()
    {
        var aggregate = new ArticleAggregate();
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
