namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public static class ValidStructureRelationshipSpecification
{
    public static bool IsSatisfied(
        StructureType fromType,
        StructureType toType,
        RelationshipType relationshipType)
    {
        if (relationshipType == RelationshipType.Owns)
            return fromType == StructureType.Holding;

        if (relationshipType == RelationshipType.Governs)
            return fromType == StructureType.Authority;

        if (relationshipType == RelationshipType.ProvidesServiceTo)
            return fromType == StructureType.Provider && toType == StructureType.Spv;

        if (relationshipType == RelationshipType.Funds)
            return fromType == StructureType.Spv && toType == StructureType.Spv;

        if (relationshipType == RelationshipType.Operates)
            return true;

        return false;
    }
}
