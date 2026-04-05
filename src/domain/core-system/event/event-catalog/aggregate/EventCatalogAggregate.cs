namespace Whycespace.Domain.CoreSystem.Event.EventCatalog;

public sealed class EventCatalogAggregate
{
    public static EventCatalogAggregate Create()
    {
        var aggregate = new EventCatalogAggregate();
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
