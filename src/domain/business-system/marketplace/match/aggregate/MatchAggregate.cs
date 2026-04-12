namespace Whycespace.Domain.BusinessSystem.Marketplace.Match;

public sealed class MatchAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MatchId Id { get; private set; }
    public MatchSideReference SideA { get; private set; }
    public MatchSideReference SideB { get; private set; }
    public MatchStatus Status { get; private set; }
    public int Version { get; private set; }

    private MatchAggregate() { }

    public static MatchAggregate Create(MatchId id, MatchSideReference sideA, MatchSideReference sideB)
    {
        var specification = new HasValidReferencesSpecification();
        if (!specification.IsSatisfiedBy(sideA, sideB))
        {
            if (sideA == default)
                throw MatchErrors.MissingSideA();
            if (sideB == default)
                throw MatchErrors.MissingSideB();
            if (sideA == sideB)
                throw MatchErrors.SidesCannotBeEqual(sideA);
        }

        var aggregate = new MatchAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MatchCreatedEvent(id, sideA, sideB);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    private void Apply(MatchCreatedEvent @event)
    {
        Id = @event.MatchId;
        SideA = @event.SideA;
        SideB = @event.SideB;
        Status = MatchStatus.Matched;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MatchErrors.MissingId();

        if (SideA == default)
            throw MatchErrors.MissingSideA();

        if (SideB == default)
            throw MatchErrors.MissingSideB();

        if (SideA == SideB)
            throw MatchErrors.SidesCannotBeEqual(SideA);

        if (!Enum.IsDefined(Status))
            throw new InvalidOperationException("MatchStatus is not a defined enum value.");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
