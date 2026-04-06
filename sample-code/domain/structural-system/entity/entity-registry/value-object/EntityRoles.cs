namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityRoles
{
    private readonly HashSet<EntityRole> _roles = new();

    public IReadOnlyCollection<EntityRole> Values => _roles;

    public void Add(EntityRole role)
    {
        _roles.Add(role);
    }

    public void Remove(EntityRole role)
    {
        _roles.Remove(role);
    }

    public bool Contains(EntityRole role) => _roles.Contains(role);
}
