namespace Whycespace.Domain.IntelligenceSystem.Knowledge.Answer;

public sealed class AnswerAggregate
{
    public static AnswerAggregate Create()
    {
        var aggregate = new AnswerAggregate();
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
