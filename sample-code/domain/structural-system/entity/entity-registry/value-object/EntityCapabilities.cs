namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityCapabilities
{
    private readonly HashSet<EntityCapability> _capabilities = new();

    public IReadOnlyCollection<EntityCapability> Values => _capabilities;

    public void Add(EntityCapability capability)
    {
        _capabilities.Add(capability);
    }

    public bool Contains(EntityCapability capability) => _capabilities.Contains(capability);
}
