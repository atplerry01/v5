using DomainEntity = Whycespace.Domain.SharedKernel.Primitives.Kernel.Entity;

namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed class StructureRelationship : DomainEntity
{
    public StructureId FromId { get; private set; } = default!;
    public StructureId ToId { get; private set; } = default!;
    public RelationshipType Type { get; private set; } = default!;

    private StructureRelationship() { }

    public static StructureRelationship Create(
        Guid id,
        StructureId fromId,
        StructureId toId,
        RelationshipType type)
    {
        return new StructureRelationship
        {
            Id = id,
            FromId = fromId,
            ToId = toId,
            Type = type
        };
    }

    public bool Matches(StructureId from, StructureId to, RelationshipType type)
        => FromId == from && ToId == to && Type == type;
}
