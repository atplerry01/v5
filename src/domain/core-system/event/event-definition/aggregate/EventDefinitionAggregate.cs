namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed class EventDefinitionAggregate
{
    public static EventDefinitionAggregate Create()
    {
        var aggregate = new EventDefinitionAggregate();
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
