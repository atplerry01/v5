using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

public sealed class EconomicSubjectAggregate : AggregateRoot
{
    public SubjectId SubjectId { get; private set; }
    public SubjectType SubjectType { get; private set; }
    public StructuralRef StructuralRef { get; private set; } = null!;
    public EconomicRef EconomicRef { get; private set; } = null!;
    public bool IsRegistered { get; private set; }

    private EconomicSubjectAggregate() { }

    public static EconomicSubjectAggregate Register(
        SubjectId subjectId,
        SubjectType subjectType,
        StructuralRef structuralRef,
        EconomicRef economicRef)
    {
        if (structuralRef is null)
            throw SubjectErrors.MissingStructuralRef();
        if (economicRef is null)
            throw SubjectErrors.MissingEconomicRef();

        EconomicRefRules.Validate(subjectType, economicRef.RefType);

        var aggregate = new EconomicSubjectAggregate();
        aggregate.RaiseDomainEvent(new EconomicSubjectRegisteredEvent(
            subjectId,
            subjectType,
            structuralRef,
            economicRef));
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
        if (!IsRegistered)
            return;

        if (StructuralRef is null)
            throw SubjectErrors.MissingStructuralRef();

        if (EconomicRef is null)
            throw SubjectErrors.MissingEconomicRef();
    }
}
