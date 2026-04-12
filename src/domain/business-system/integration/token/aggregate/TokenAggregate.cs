namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public sealed class TokenAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TokenId Id { get; private set; }
    public TokenDescriptor Descriptor { get; private set; }
    public TokenStatus Status { get; private set; }
    public int Version { get; private set; }

    private TokenAggregate() { }

    public static TokenAggregate Issue(TokenId id, TokenDescriptor descriptor)
    {
        var aggregate = new TokenAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TokenIssuedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TokenErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TokenActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire()
    {
        ValidateBeforeChange();

        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TokenErrors.InvalidStateTransition(Status, nameof(Expire));

        var @event = new TokenExpiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke()
    {
        ValidateBeforeChange();

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TokenErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new TokenRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TokenIssuedEvent @event)
    {
        Id = @event.TokenId;
        Descriptor = @event.Descriptor;
        Status = TokenStatus.Issued;
        Version++;
    }

    private void Apply(TokenActivatedEvent @event)
    {
        Status = TokenStatus.Active;
        Version++;
    }

    private void Apply(TokenExpiredEvent @event)
    {
        Status = TokenStatus.Expired;
        Version++;
    }

    private void Apply(TokenRevokedEvent @event)
    {
        Status = TokenStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw TokenErrors.MissingId();

        if (Descriptor == default)
            throw TokenErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TokenErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
