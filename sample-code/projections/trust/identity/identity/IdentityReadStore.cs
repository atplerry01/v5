using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Identity;

public sealed class IdentityReadStore
{
    private const string IdentityProjection = "identity";
    private const string SessionProjection = "identity.session";
    private const string RoleProjection = "identity.role";
    private const string DeviceProjection = "identity.device";
    private const string AccessProfileProjection = "identity.access-profile";
    private const string MetricsKey = "identity:metrics";

    private readonly IProjectionStore _store;
    private readonly IClock _clock;

    public IdentityReadStore(IProjectionStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    // Identity
    public Task<IdentityReadModel?> GetIdentityAsync(string identityId, CancellationToken ct = default)
        => _store.GetAsync<IdentityReadModel>(IdentityProjection, identityId, ct);

    public Task SetIdentityAsync(string identityId, IdentityReadModel model, CancellationToken ct = default)
        => _store.SetAsync(IdentityProjection, identityId, model, ct);

    public Task<IReadOnlyList<IdentityReadModel>> GetAllIdentitiesAsync(CancellationToken ct = default)
        => _store.GetAllAsync<IdentityReadModel>(IdentityProjection, ct);

    // Session
    public Task<IdentitySessionReadModel?> GetSessionAsync(string sessionId, CancellationToken ct = default)
        => _store.GetAsync<IdentitySessionReadModel>(SessionProjection, sessionId, ct);

    public Task SetSessionAsync(string sessionId, IdentitySessionReadModel model, CancellationToken ct = default)
        => _store.SetAsync(SessionProjection, sessionId, model, ct);

    public Task RemoveSessionAsync(string sessionId, CancellationToken ct = default)
        => _store.RemoveAsync(SessionProjection, sessionId, ct);

    // Role
    public Task<IdentityRoleReadModel?> GetRoleAsync(string roleId, CancellationToken ct = default)
        => _store.GetAsync<IdentityRoleReadModel>(RoleProjection, roleId, ct);

    public Task SetRoleAsync(string roleId, IdentityRoleReadModel model, CancellationToken ct = default)
        => _store.SetAsync(RoleProjection, roleId, model, ct);

    public Task<IReadOnlyList<IdentityRoleReadModel>> GetAllRolesAsync(CancellationToken ct = default)
        => _store.GetAllAsync<IdentityRoleReadModel>(RoleProjection, ct);

    // Device
    public Task<IdentityDeviceReadModel?> GetDeviceAsync(string deviceId, CancellationToken ct = default)
        => _store.GetAsync<IdentityDeviceReadModel>(DeviceProjection, deviceId, ct);

    public Task SetDeviceAsync(string deviceId, IdentityDeviceReadModel model, CancellationToken ct = default)
        => _store.SetAsync(DeviceProjection, deviceId, model, ct);

    // AccessProfile
    public Task<IdentityAccessProfileReadModel?> GetAccessProfileAsync(string profileId, CancellationToken ct = default)
        => _store.GetAsync<IdentityAccessProfileReadModel>(AccessProfileProjection, profileId, ct);

    public Task SetAccessProfileAsync(string profileId, IdentityAccessProfileReadModel model, CancellationToken ct = default)
        => _store.SetAsync(AccessProfileProjection, profileId, model, ct);

    // Metrics
    public Task<IdentityMetricsReadModel?> GetMetricsAsync(CancellationToken ct = default)
        => _store.GetAsync<IdentityMetricsReadModel>(IdentityProjection, MetricsKey, ct);

    public Task SetMetricsAsync(IdentityMetricsReadModel model, CancellationToken ct = default)
        => _store.SetAsync(IdentityProjection, MetricsKey, model, ct);

    /// <summary>
    /// Clears all identity projection data. Used by rebuilder before replay.
    /// Iterates all known entries and removes them, then resets metrics.
    /// </summary>
    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        // Clear identities
        var identities = await _store.GetAllAsync<IdentityReadModel>(IdentityProjection, ct);
        foreach (var identity in identities)
            await _store.RemoveAsync(IdentityProjection, identity.IdentityId, ct);

        // Clear sessions
        var sessions = await _store.GetAllAsync<IdentitySessionReadModel>(SessionProjection, ct);
        foreach (var session in sessions)
            await _store.RemoveAsync(SessionProjection, session.SessionId, ct);

        // Clear roles
        var roles = await _store.GetAllAsync<IdentityRoleReadModel>(RoleProjection, ct);
        foreach (var role in roles)
            await _store.RemoveAsync(RoleProjection, role.RoleId, ct);

        // Clear devices
        var devices = await _store.GetAllAsync<IdentityDeviceReadModel>(DeviceProjection, ct);
        foreach (var device in devices)
            await _store.RemoveAsync(DeviceProjection, device.DeviceId, ct);

        // Clear access profiles
        var profiles = await _store.GetAllAsync<IdentityAccessProfileReadModel>(AccessProfileProjection, ct);
        foreach (var profile in profiles)
            await _store.RemoveAsync(AccessProfileProjection, profile.ProfileId, ct);

        // Reset metrics to zero
        await SetMetricsAsync(new IdentityMetricsReadModel { LastUpdated = _clock.UtcNowOffset }, ct);

        // Reset checkpoint
        await _store.SetCheckpointAsync(IdentityProjection, 0, ct);
    }
}
