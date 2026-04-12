namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class AuthorizationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AuthorizationId Id { get; private set; }
    public AuthorizationScope Scope { get; private set; }
    public AuthorizationStatus Status { get; private set; }
    public int Version { get; private set; }

    private AuthorizationAggregate() { }

    // ── Factory: Grant ──────────────────────────────────────────

    public static AuthorizationAggregate Grant(
        AuthorizationId id,
        AuthorizationScope scope)
    {
        var aggregate = new AuthorizationAggregate();

        var @event = new AuthorizationGrantedEvent(id, scope);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Factory: Deny ───────────────────────────────────────────

    public static AuthorizationAggregate Deny(
        AuthorizationId id,
        AuthorizationScope scope)
    {
        var aggregate = new AuthorizationAggregate();

        var @event = new AuthorizationDeniedEvent(id, scope);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Transition: Revoke ──────────────────────────────────────

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorizationErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new AuthorizationRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ───────────────────────────────────────────────────

    private void Apply(AuthorizationGrantedEvent @event)
    {
        Id = @event.AuthorizationId;
        Scope = @event.Scope;
        Status = AuthorizationStatus.Granted;
        Version++;
    }

    private void Apply(AuthorizationDeniedEvent @event)
    {
        Id = @event.AuthorizationId;
        Scope = @event.Scope;
        Status = AuthorizationStatus.Denied;
        Version++;
    }

    private void Apply(AuthorizationRevokedEvent @event)
    {
        Status = AuthorizationStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AuthorizationErrors.MissingId();

        if (Scope == default)
            throw AuthorizationErrors.MissingScope();

        if (!Enum.IsDefined(Status))
            throw AuthorizationErrors.InvalidStateTransition(Status, "validate");
    }
}
