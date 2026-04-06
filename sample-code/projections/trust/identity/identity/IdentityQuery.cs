namespace Whycespace.Projections.Identity;

public sealed class IdentityQuery
{
    private readonly IdentityReadStore _store;

    public IdentityQuery(IdentityReadStore store) => _store = store;

    public Task<IdentityReadModel?> GetIdentityAsync(string identityId, CancellationToken ct = default)
        => _store.GetIdentityAsync(identityId, ct);

    public Task<IReadOnlyList<IdentityReadModel>> ListIdentitiesAsync(CancellationToken ct = default)
        => _store.GetAllIdentitiesAsync(ct);

    public Task<IdentitySessionReadModel?> GetSessionAsync(string sessionId, CancellationToken ct = default)
        => _store.GetSessionAsync(sessionId, ct);

    public Task<IdentityRoleReadModel?> GetRoleAsync(string roleId, CancellationToken ct = default)
        => _store.GetRoleAsync(roleId, ct);

    public Task<IReadOnlyList<IdentityRoleReadModel>> ListRolesAsync(CancellationToken ct = default)
        => _store.GetAllRolesAsync(ct);

    public Task<IdentityDeviceReadModel?> GetDeviceAsync(string deviceId, CancellationToken ct = default)
        => _store.GetDeviceAsync(deviceId, ct);

    public Task<IdentityAccessProfileReadModel?> GetAccessProfileAsync(string profileId, CancellationToken ct = default)
        => _store.GetAccessProfileAsync(profileId, ct);

    public Task<IdentityMetricsReadModel?> GetMetricsAsync(CancellationToken ct = default)
        => _store.GetMetricsAsync(ct);
}
