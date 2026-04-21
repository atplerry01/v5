using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public sealed class ClassificationAggregate : AggregateRoot
{
    public ClassificationId Id { get; private set; }
    public ClassificationDescriptor Descriptor { get; private set; }
    public ClassificationStatus Status { get; private set; }

    public static ClassificationAggregate Define(ClassificationId id, ClassificationDescriptor descriptor)
    {
        var aggregate = new ClassificationAggregate();
        if (aggregate.Version >= 0)
            throw ClassificationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ClassificationDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClassificationErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ClassificationActivatedEvent(Id));
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClassificationErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new ClassificationDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ClassificationDefinedEvent e:
                Id = e.ClassificationId;
                Descriptor = e.Descriptor;
                Status = ClassificationStatus.Defined;
                break;
            case ClassificationActivatedEvent:
                Status = ClassificationStatus.Active;
                break;
            case ClassificationDeprecatedEvent:
                Status = ClassificationStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ClassificationErrors.MissingId();

        if (Descriptor == default)
            throw ClassificationErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw ClassificationErrors.InvalidStateTransition(Status, "validate");
    }
}
