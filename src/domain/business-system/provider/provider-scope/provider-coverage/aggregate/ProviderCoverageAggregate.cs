using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed class ProviderCoverageAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly HashSet<CoverageScope> _scopes = new();

    public ProviderCoverageId Id { get; private set; }
    public ProviderRef Provider { get; private set; }
    public ProviderCoverageStatus Status { get; private set; }
    public IReadOnlyCollection<CoverageScope> Scopes => _scopes;
    public int Version { get; private set; }

    private ProviderCoverageAggregate() { }

    public static ProviderCoverageAggregate Create(ProviderCoverageId id, ProviderRef provider)
    {
        var aggregate = new ProviderCoverageAggregate();

        var @event = new ProviderCoverageCreatedEvent(id, provider);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddScope(CoverageScope scope)
    {
        EnsureMutable();

        if (_scopes.Contains(scope))
            throw ProviderCoverageErrors.ScopeAlreadyPresent(scope);

        var @event = new CoverageScopeAddedEvent(Id, scope);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveScope(CoverageScope scope)
    {
        EnsureMutable();

        if (!_scopes.Contains(scope))
            throw ProviderCoverageErrors.ScopeNotPresent(scope);

        var @event = new CoverageScopeRemovedEvent(Id, scope);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status, _scopes.Count))
        {
            if (Status != ProviderCoverageStatus.Draft)
                throw ProviderCoverageErrors.InvalidStateTransition(Status, nameof(Activate));
            throw ProviderCoverageErrors.ActivationRequiresScope();
        }

        var @event = new ProviderCoverageActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProviderCoverageStatus.Archived)
            throw ProviderCoverageErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProviderCoverageArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderCoverageCreatedEvent @event)
    {
        Id = @event.ProviderCoverageId;
        Provider = @event.Provider;
        Status = ProviderCoverageStatus.Draft;
        Version++;
    }

    private void Apply(CoverageScopeAddedEvent @event)
    {
        _scopes.Add(@event.Scope);
        Version++;
    }

    private void Apply(CoverageScopeRemovedEvent @event)
    {
        _scopes.Remove(@event.Scope);
        Version++;
    }

    private void Apply(ProviderCoverageActivatedEvent @event)
    {
        Status = ProviderCoverageStatus.Active;
        Version++;
    }

    private void Apply(ProviderCoverageArchivedEvent @event)
    {
        Status = ProviderCoverageStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCoverageErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderCoverageErrors.MissingId();

        if (Provider == default)
            throw ProviderCoverageErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderCoverageErrors.InvalidStateTransition(Status, "validate");
    }
}
