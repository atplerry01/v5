namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityRelationship : IEquatable<EntityRelationship>
{
    public Guid FromEntityId { get; }
    public Guid ToEntityId { get; }
    public string RelationshipType { get; }

    public EntityRelationship(Guid from, Guid to, string type)
    {
        FromEntityId = from;
        ToEntityId = to;
        RelationshipType = type;
    }

    public bool Equals(EntityRelationship? other)
    {
        return other is not null &&
               FromEntityId == other.FromEntityId &&
               ToEntityId == other.ToEntityId &&
               RelationshipType == other.RelationshipType;
    }

    public override bool Equals(object? obj) => obj is EntityRelationship other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(FromEntityId, ToEntityId, RelationshipType);
}
