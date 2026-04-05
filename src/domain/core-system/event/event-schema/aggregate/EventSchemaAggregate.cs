namespace Whycespace.Domain.CoreSystem.Event.EventSchema;

public sealed class EventSchemaAggregate
{
    public static EventSchemaAggregate Create()
    {
        var aggregate = new EventSchemaAggregate();
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
