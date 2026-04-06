namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public static class RoleCapabilityMapping
{
    public static IEnumerable<EntityCapability> Map(EntityRole role)
    {
        if (role.Equals(EntityRole.Investor))
            return new[] { EntityCapability.CapitalPooling, EntityCapability.AssetHolding };

        if (role.Equals(EntityRole.Operator))
            return new[] { EntityCapability.Execution, EntityCapability.ServiceProvision };

        if (role.Equals(EntityRole.Brand))
            return new[] { EntityCapability.CustomerInterface, EntityCapability.RevenueInterface };

        return Enumerable.Empty<EntityCapability>();
    }
}
