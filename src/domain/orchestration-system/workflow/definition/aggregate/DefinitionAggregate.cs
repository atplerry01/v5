using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public sealed class DefinitionAggregate : AggregateRoot
{
    public DefinitionId Id { get; private set; }
    private WorkflowBlueprint _blueprint;
    private DefinitionStatus _status;

    private DefinitionAggregate() { }

    public static DefinitionAggregate Draft(DefinitionId id, WorkflowBlueprint blueprint)
    {
        var aggregate = new DefinitionAggregate();
        aggregate.RaiseDomainEvent(new DefinitionDraftedEvent(id, blueprint));
        return aggregate;
    }

    public void Publish()
    {
        if (!CanPublishSpecification.IsSatisfiedBy(_status))
            throw DefinitionErrors.InvalidStateTransition(_status, "publish");

        RaiseDomainEvent(new DefinitionPublishedEvent(Id));
    }

    public void Retire()
    {
        if (!CanRetireSpecification.IsSatisfiedBy(_status))
            throw DefinitionErrors.InvalidStateTransition(_status, "retire");

        RaiseDomainEvent(new DefinitionRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DefinitionDraftedEvent e:
                Id = e.DefinitionId;
                _blueprint = e.Blueprint;
                _status = DefinitionStatus.Draft;
                break;

            case DefinitionPublishedEvent:
                _status = DefinitionStatus.Published;
                break;

            case DefinitionRetiredEvent:
                _status = DefinitionStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        // Terminal lifecycle: Draft -> Published -> Retired
        // Invariants enforced by specifications before transitions
    }
}
