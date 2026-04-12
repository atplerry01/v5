namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed record TypeDefinitionDefinedEvent(TypeDefinitionId TypeDefinitionId, TypeDefinitionDescriptor Descriptor);

public sealed record TypeDefinitionActivatedEvent(TypeDefinitionId TypeDefinitionId);

public sealed record TypeDefinitionRetiredEvent(TypeDefinitionId TypeDefinitionId);
