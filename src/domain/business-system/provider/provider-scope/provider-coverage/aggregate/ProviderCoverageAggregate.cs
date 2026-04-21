using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public sealed class ProviderCoverageAggregate : AggregateRoot
{
    private readonly HashSet<CoverageScope> _scopes = new();

    public ProviderCoverageId Id { get; private set; }
    public ClusterProviderRef Provider { get; private set; }
    public ProviderCoverageStatus Status { get; private set; }
    public IReadOnlyCollection<CoverageScope> Scopes => _scopes;

    public static ProviderCoverageAggregate Create(ProviderCoverageId id, ClusterProviderRef provider)
    {
        var aggregate = new ProviderCoverageAggregate();
        if (aggregate.Version >= 0)
            throw ProviderCoverageErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderCoverageCreatedEvent(id, provider));
        return aggregate;
    }

    public void AddScope(CoverageScope scope)
    {
        EnsureMutable();

        if (_scopes.Contains(scope))
            throw ProviderCoverageErrors.ScopeAlreadyPresent(scope);

        RaiseDomainEvent(new CoverageScopeAddedEvent(Id, scope));
    }

    public void RemoveScope(CoverageScope scope)
    {
        EnsureMutable();

        if (!_scopes.Contains(scope))
            throw ProviderCoverageErrors.ScopeNotPresent(scope);

        RaiseDomainEvent(new CoverageScopeRemovedEvent(Id, scope));
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

        RaiseDomainEvent(new ProviderCoverageActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ProviderCoverageStatus.Archived)
            throw ProviderCoverageErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProviderCoverageArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderCoverageCreatedEvent e:
                Id = e.ProviderCoverageId;
                Provider = e.Provider;
                Status = ProviderCoverageStatus.Draft;
                break;
            case CoverageScopeAddedEvent e:
                _scopes.Add(e.Scope);
                break;
            case CoverageScopeRemovedEvent e:
                _scopes.Remove(e.Scope);
                break;
            case ProviderCoverageActivatedEvent:
                Status = ProviderCoverageStatus.Active;
                break;
            case ProviderCoverageArchivedEvent:
                Status = ProviderCoverageStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCoverageErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderCoverageErrors.MissingId();

        if (Provider == default)
            throw ProviderCoverageErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderCoverageErrors.InvalidStateTransition(Status, "validate");
    }
}
