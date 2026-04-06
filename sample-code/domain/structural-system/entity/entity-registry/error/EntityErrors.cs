namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityException : DomainException
{
    public EntityException(string message) : base("ENTITY_ERROR", message) { }
}
