using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public sealed class VersioningRuleAggregate : AggregateRoot
{
    public VersioningRuleId VersioningRuleId { get; private set; }
    public Guid SchemaRef { get; private set; }
    public int FromVersion { get; private set; }
    public int ToVersion { get; private set; }
    public EvolutionClass EvolutionClass { get; private set; }
    public IReadOnlyList<SchemaChange> ChangeSummary { get; private set; } = [];
    public CompatibilityVerdict? Verdict { get; private set; }

    private VersioningRuleAggregate() { }

    public static VersioningRuleAggregate Register(
        VersioningRuleId id,
        Guid schemaRef,
        int fromVersion,
        int toVersion,
        EvolutionClass evolutionClass,
        IReadOnlyList<SchemaChange> changeSummary,
        Timestamp registeredAt)
    {
        var aggregate = new VersioningRuleAggregate();
        if (aggregate.Version >= 0)
            throw VersioningRuleErrors.AlreadyInitialized();

        if (schemaRef == Guid.Empty)
            throw VersioningRuleErrors.SchemaRefMissing();

        if (toVersion <= fromVersion)
            throw VersioningRuleErrors.InvalidVersionOrder();

        if (changeSummary is null || changeSummary.Count == 0)
            throw VersioningRuleErrors.EmptyChangeSummary();

        aggregate.RaiseDomainEvent(new VersioningRuleRegisteredEvent(
            id, schemaRef, fromVersion, toVersion, evolutionClass, changeSummary, registeredAt));

        return aggregate;
    }

    public void IssueVerdict(CompatibilityVerdict verdict, Timestamp issuedAt)
    {
        if (Verdict.HasValue)
            throw VersioningRuleErrors.VerdictAlreadyIssued();

        RaiseDomainEvent(new VersioningRuleVerdictIssuedEvent(VersioningRuleId, verdict, issuedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case VersioningRuleRegisteredEvent e:
                VersioningRuleId = e.VersioningRuleId;
                SchemaRef = e.SchemaRef;
                FromVersion = e.FromVersion;
                ToVersion = e.ToVersion;
                EvolutionClass = e.EvolutionClass;
                ChangeSummary = e.ChangeSummary;
                break;

            case VersioningRuleVerdictIssuedEvent e:
                Verdict = e.Verdict;
                break;
        }
    }
}
