namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

/// <summary>
/// Defines the scope of an emergency halt: global, region, or SPV.
/// </summary>
public sealed record EmergencyScope
{
    public string ScopeType { get; }
    public string TargetId { get; }

    private EmergencyScope(string scopeType, string targetId)
    {
        ScopeType = scopeType;
        TargetId = targetId;
    }

    public static EmergencyScope Global() => new("Global", "*");
    public static EmergencyScope Region(string regionId) => new("Region", regionId);
    public static EmergencyScope Spv(string spvId) => new("Spv", spvId);
}
