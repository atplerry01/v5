namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public static class ValidEntityRelationshipSpecification
{
    public static bool IsSatisfied(string fromType, string toType, string relationship)
    {
        if (relationship == "FUNDS")
            return fromType == "SPV" && toType == "SPV";

        if (relationship == "PROVIDES_SERVICE_TO")
            return fromType == "PROVIDER" && toType == "SPV";

        if (relationship == "DELIVERS_TO")
            return fromType == "SPV" && toType == "SPV";

        if (relationship == "GOVERNS")
            return fromType == "AUTHORITY";

        if (relationship == "OWNS")
            return fromType == "HOLDING";

        return true;
    }
}
