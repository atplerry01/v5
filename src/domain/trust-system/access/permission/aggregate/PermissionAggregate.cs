namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed class PermissionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PermissionId Id { get; private set; }
    public PermissionDescriptor Descriptor { get; private set; }
    public PermissionStatus Status { get; private set; }
    public int Version { get; private set; }

    private PermissionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static PermissionAggregate Define(
        PermissionId id,
        PermissionDescriptor descriptor)
    {
        var aggregate = new PermissionAggregate();

        var @event = new PermissionDefinedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PermissionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PermissionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PermissionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new PermissionDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(PermissionDefinedEvent @event)
    {
        Id = @event.PermissionId;
        Descriptor = @event.Descriptor;
        Status = PermissionStatus.Defined;
        Version++;
    }

    private void Apply(PermissionActivatedEvent @event)
    {
        Status = PermissionStatus.Active;
        Version++;
    }

    private void Apply(PermissionDeprecatedEvent @event)
    {
        Status = PermissionStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PermissionErrors.MissingId();

        if (Descriptor == default)
            throw PermissionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw PermissionErrors.InvalidStateTransition(Status, "validate");
    }
}
