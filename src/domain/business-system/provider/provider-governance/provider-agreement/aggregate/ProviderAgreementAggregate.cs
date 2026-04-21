using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public sealed class ProviderAgreementAggregate : AggregateRoot
{
    public ProviderAgreementId Id { get; private set; }
    public ClusterProviderRef Provider { get; private set; }
    public ContractRef? Contract { get; private set; }
    public ProviderAgreementStatus Status { get; private set; }
    public TimeWindow? Effective { get; private set; }

    public static ProviderAgreementAggregate Create(
        ProviderAgreementId id,
        ClusterProviderRef provider,
        ContractRef? contract = null,
        TimeWindow? effective = null)
    {
        var aggregate = new ProviderAgreementAggregate();
        if (aggregate.Version >= 0)
            throw ProviderAgreementErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderAgreementCreatedEvent(id, provider, contract, effective));
        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProviderAgreementActivatedEvent(Id, effective));
    }

    public void Suspend(DateTimeOffset suspendedAt)
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new ProviderAgreementSuspendedEvent(Id, suspendedAt));
    }

    public void Terminate(DateTimeOffset terminatedAt)
    {
        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAgreementErrors.AlreadyTerminated(Id);

        RaiseDomainEvent(new ProviderAgreementTerminatedEvent(Id, terminatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderAgreementCreatedEvent e:
                Id = e.ProviderAgreementId;
                Provider = e.Provider;
                Contract = e.Contract;
                Effective = e.Effective;
                Status = ProviderAgreementStatus.Draft;
                break;
            case ProviderAgreementActivatedEvent e:
                Effective = e.Effective;
                Status = ProviderAgreementStatus.Active;
                break;
            case ProviderAgreementSuspendedEvent:
                Status = ProviderAgreementStatus.Suspended;
                break;
            case ProviderAgreementTerminatedEvent:
                Status = ProviderAgreementStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
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
