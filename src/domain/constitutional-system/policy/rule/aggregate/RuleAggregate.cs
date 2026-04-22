using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class RuleAggregate : AggregateRoot
{
    public RuleId Id { get; private set; }
    public RuleDefinition Definition { get; private set; }
    public RuleStatus Status { get; private set; }

    private RuleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RuleAggregate Draft(
        RuleId id,
        RuleDefinition definition)
    {
        var aggregate = new RuleAggregate();
        aggregate.RaiseDomainEvent(new RuleDraftedEvent(id, definition));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RuleErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new RuleActivatedEvent(Id));
    }

    // ── Retire ───────────────────────────────────────────────────

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RuleErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new RuleRetiredEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RuleDraftedEvent e:
                Id = e.RuleId;
                Definition = e.Definition;
                Status = RuleStatus.Draft;
                break;
            case RuleActivatedEvent:
                Status = RuleStatus.Active;
                break;
            case RuleRetiredEvent:
                Status = RuleStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw RuleErrors.MissingId();

        if (Definition == default)
            throw RuleErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw RuleErrors.InvalidStateTransition(Status, "validate");
    }
}
