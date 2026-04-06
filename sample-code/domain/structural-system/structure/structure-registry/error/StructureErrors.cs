namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed class StructureNotFoundException : DomainException
{
    public StructureNotFoundException(StructureId id)
        : base("STRUCTURE_NOT_FOUND", $"Structure not found: {id.Value}") { }
}

public sealed class InvalidStructureRelationshipException : DomainException
{
    public InvalidStructureRelationshipException(string from, string to, string relationship)
        : base("INVALID_STRUCTURE_RELATIONSHIP",
            $"Invalid relationship '{relationship}' between '{from}' and '{to}'") { }
}
