using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed class ProviderAgreementAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderAgreementId Id { get; private set; }
    public ProviderRef Provider { get; private set; }
    public ContractRef? Contract { get; private set; }
    public ProviderAgreementStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }
    public int Version { get; private set; }

    private ProviderAgreementAggregate() { }

    public static ProviderAgreementAggregate Create(
        ProviderAgreementId id,
        ProviderRef provider,
        ContractRef? contract = null,
        TimeWindow? effective = null)
    {
        var aggregate = new ProviderAgreementAggregate();

        var @event = new ProviderAgreementCreatedEvent(id, provider, contract, effective);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderAgreementActivatedEvent(Id, effective);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend(DateTimeOffset suspendedAt)
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ProviderAgreementSuspendedEvent(Id, suspendedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate(DateTimeOffset terminatedAt)
    {
        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.AlreadyTerminated(Id);

        var @event = new ProviderAgreementTerminatedEvent(Id, terminatedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderAgreementCreatedEvent @event)
    {
        Id = @event.ProviderAgreementId;
        Provider = @event.Provider;
        Contract = @event.Contract;
        Effective = @event.Effective;
        Status = ProviderAgreementStatus.Draft;
        Version++;
    }

    private void Apply(ProviderAgreementActivatedEvent @event)
    {
        Effective = @event.Effective;
        Status = ProviderAgreementStatus.Active;
        Version++;
    }

    private void Apply(ProviderAgreementSuspendedEvent @event)
    {
        Status = ProviderAgreementStatus.Suspended;
        Version++;
    }

    private void Apply(ProviderAgreementTerminatedEvent @event)
    {
        Status = ProviderAgreementStatus.Terminated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderAgreementErrors.MissingId();

        if (Provider == default)
            throw ProviderAgreementErrors.MissingProviderRef();

        if (Status == ProviderAgreementStatus.Active && Effective is null)
            throw ProviderAgreementErrors.ActivationRequiresEffectiveWindow();

        if (!Enum.IsDefined(Status))
            throw ProviderAgreementErrors.InvalidStateTransition(Status, "validate");
    }
}
