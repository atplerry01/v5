namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class ValidRoleForEntityTypeSpecification
{
    public static bool IsSatisfied(string entityType, string role)
    {
        if (entityType == "SPV")
        {
            return role is "INVESTOR" or "OPERATOR" or "BRAND";
        }

        return false;
    }
}
