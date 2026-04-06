namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public static class ValidEntityBindingSpecification
{
    public static bool CanBindToEconomic(string entityType)
        => entityType == "SPV";

    public static bool CanBindToWorkflow(string entityType)
        => entityType is "SPV" or "PROVIDER";

    public static bool CanBindToGovernance(string entityType)
        => true;

    public static bool CanBindToIdentity(string entityType)
        => true;
}
