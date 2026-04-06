namespace Whycespace.Domain.EconomicSystem.Enforcement.Enforcement;

public static class EnforcementErrors
{
    public static DomainException AlreadyActive(Guid enforcementId) =>
        new("ENFORCEMENT_ALREADY_ACTIVE", $"Enforcement {enforcementId} is already active.");

    public static DomainException AlreadyReleased(Guid enforcementId) =>
        new("ENFORCEMENT_ALREADY_RELEASED", $"Enforcement {enforcementId} has already been released.");

    public static DomainException InvalidType(string type) =>
        new("ENFORCEMENT_INVALID_TYPE", $"Invalid enforcement type: {type}.");
}
