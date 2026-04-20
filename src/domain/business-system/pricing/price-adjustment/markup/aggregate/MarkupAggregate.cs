using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public sealed class MarkupAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MarkupId Id { get; private set; }
    public MarkupCode Code { get; private set; }
    public MarkupName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public MarkupAmount Amount { get; private set; }
    public MarkupStatus Status { get; private set; }
    public int Version { get; private set; }

    private MarkupAggregate() { }

    public static MarkupAggregate Create(
        MarkupId id,
        MarkupCode code,
        MarkupName name,
        AdjustmentBasis basis,
        MarkupAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new MarkupAggregate();

        var @event = new MarkupCreatedEvent(id, code, name, basis, amount);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(MarkupName name, AdjustmentBasis basis, MarkupAmount amount)
    {
        if (Status == MarkupStatus.Archived)
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        var @event = new MarkupUpdatedEvent(Id, name, basis, amount);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new MarkupActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new MarkupDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == MarkupStatus.Archived)
            throw MarkupErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new MarkupArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, MarkupAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw MarkupErrors.PercentageOutOfRange(amount.Value);
    }

    private void Apply(MarkupCreatedEvent @event)
    {
        Id = @event.MarkupId;
        Code = @event.Code;
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Status = MarkupStatus.Draft;
        Version++;
    }

    private void Apply(MarkupUpdatedEvent @event)
    {
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Version++;
    }

    private void Apply(MarkupActivatedEvent @event)
    {
        Status = MarkupStatus.Active;
        Version++;
    }

    private void Apply(MarkupDeprecatedEvent @event)
    {
        Status = MarkupStatus.Deprecated;
        Version++;
    }

    private void Apply(MarkupArchivedEvent @event)
    {
        Status = MarkupStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MarkupErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw MarkupErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw MarkupErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
