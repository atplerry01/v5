namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed class ClientAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<ClientCredential> _credentials = new();

    public ClientId Id { get; private set; }
    public ExternalClientId ExternalId { get; private set; }
    public ClientStatus Status { get; private set; }
    public IReadOnlyList<ClientCredential> Credentials => _credentials.AsReadOnly();
    public int Version { get; private set; }

    private ClientAggregate() { }

    public static ClientAggregate Create(ClientId id, ExternalClientId externalId)
    {
        var aggregate = new ClientAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ClientCreatedEvent(id, externalId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddCredential(ClientCredential credential)
    {
        if (credential is null)
            throw new ArgumentNullException(nameof(credential));

        _credentials.Add(credential);
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClientErrors.InvalidStateTransition(Status, nameof(Activate));

        if (_credentials.Count == 0)
            throw ClientErrors.CredentialRequired();

        var @event = new ClientActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClientErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ClientSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ClientCreatedEvent @event)
    {
        Id = @event.ClientId;
        ExternalId = @event.ExternalId;
        Status = ClientStatus.Registered;
        Version++;
    }

    private void Apply(ClientActivatedEvent @event)
    {
        Status = ClientStatus.Active;
        Version++;
    }

    private void Apply(ClientSuspendedEvent @event)
    {
        Status = ClientStatus.Suspended;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ClientErrors.MissingId();

        if (ExternalId == default)
            throw ClientErrors.MissingExternalId();

        if (!Enum.IsDefined(Status))
            throw ClientErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
