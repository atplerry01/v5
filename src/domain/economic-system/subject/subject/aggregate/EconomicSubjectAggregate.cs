using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public sealed class EconomicSubjectAggregate : AggregateRoot
{
    public SubjectId SubjectId { get; private set; }
    public SubjectType SubjectType { get; private set; }
    public StructuralRef StructuralRef { get; private set; } = null!;
    public EconomicRef? EconomicRef { get; private set; }
    public bool IsRegistered { get; private set; }

    private EconomicSubjectAggregate() { }

    public static EconomicSubjectAggregate Register(
        SubjectId subjectId,
        SubjectType subjectType,
        StructuralRef structuralRef,
        EconomicRef economicRef)
    {
        if (structuralRef is null)
            throw new ArgumentException("StructuralRef cannot be null", nameof(structuralRef));

        var aggregate = new EconomicSubjectAggregate();

        aggregate.RaiseDomainEvent(new EconomicSubjectRegisteredEvent(
            subjectId,
            subjectType,
            structuralRef,
            economicRef));

        aggregate.EnsureInvariants();

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EconomicSubjectRegisteredEvent e:
                SubjectId = e.SubjectId;
                SubjectType = e.SubjectType;
                StructuralRef = e.StructuralRef;
                EconomicRef = e.EconomicRef;
                IsRegistered = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        // The base class calls EnsureInvariants BEFORE Apply on the initial
        // RaiseDomainEvent. Skip strict checks until state has been populated
        // by Apply; Register() re-invokes EnsureInvariants post-Apply so the
        // fully-constituted aggregate is always validated before returning.
        if (!IsRegistered)
            return;

        if (StructuralRef is null)
            throw new InvalidOperationException("StructuralRef must be set");

        if (EconomicRef is null)
            throw new InvalidOperationException("EconomicRef must be set");
    }
}
