namespace Whycespace.Projections.Identity;

public sealed record IdentityMetricsReadModel
{
    public int TotalIdentities { get; init; }
    public int ActiveIdentities { get; init; }
    public int SuspendedIdentities { get; init; }
    public int DeactivatedIdentities { get; init; }
    public int ActiveSessions { get; init; }
    public int TotalDevices { get; init; }
    public int TotalRoles { get; init; }
    public int TotalPermissions { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
