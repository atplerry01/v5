using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed class TypeDefinitionAggregate : AggregateRoot
{
    public TypeDefinitionId Id { get; private set; }
    public TypeDefinitionDescriptor Descriptor { get; private set; }
    public TypeDefinitionStatus Status { get; private set; }

    public static TypeDefinitionAggregate Define(TypeDefinitionId id, TypeDefinitionDescriptor descriptor)
    {
        var aggregate = new TypeDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw TypeDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new TypeDefinitionDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TypeDefinitionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new TypeDefinitionActivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TypeDefinitionErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new TypeDefinitionRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TypeDefinitionDefinedEvent e:
                Id = e.TypeDefinitionId;
                Descriptor = e.Descriptor;
                Status = TypeDefinitionStatus.Defined;
                break;
            case TypeDefinitionActivatedEvent:
                Status = TypeDefinitionStatus.Active;
                break;
            case TypeDefinitionRetiredEvent:
                Status = TypeDefinitionStatus.Retired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw TypeDefinitionErrors.MissingId();

        if (Descriptor == default)
            throw TypeDefinitionErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TypeDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
