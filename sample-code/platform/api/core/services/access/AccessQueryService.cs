using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Access;

namespace Whycespace.Platform.Api.Core.Services.Access;

/// <summary>
/// Projection-backed access query service.
/// Reads identity access state from WhyceID projections.
/// Pure read-only — no role assignment, no permission modification.
/// </summary>
public sealed class AccessQueryService : IAccessQueryService
{
    private readonly ProjectionAdapter _projections;

    public AccessQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<IdentityAccessView?> GetIdentityAccessAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<IdentityAccessView>(
            "identity.access",
            new Dictionary<string, object> { ["identityId"] = identityId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return null;
        return response.Data as IdentityAccessView;
    }
}
