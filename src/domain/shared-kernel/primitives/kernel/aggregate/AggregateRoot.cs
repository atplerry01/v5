namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public abstract class AggregateRoot
{
    private readonly List<object> _domainEvents = new();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();
    public int Version { get; private set; } = -1;

    protected void RaiseDomainEvent(object domainEvent)
    {
        ValidateBeforeChange();
        EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        Apply(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void LoadFromHistory(IEnumerable<object> history)
    {
        foreach (var e in history)
        {
            Apply(e);
            Version++;
        }
    }

    /// <summary>
    /// Apply an event to mutate aggregate state. Override in derived aggregates.
    /// </summary>
    protected virtual void Apply(object domainEvent) { }

    protected virtual void EnsureInvariants()
    {
        // Override in derived aggregates to enforce domain invariants
    }

    protected virtual void ValidateBeforeChange()
    {
        // Override in derived aggregates for pre-change validation
    }
}
