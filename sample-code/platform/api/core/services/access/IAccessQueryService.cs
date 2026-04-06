using Whycespace.Platform.Api.Core.Contracts.Access;

namespace Whycespace.Platform.Api.Core.Services.Access;

/// <summary>
/// Read-only access query service.
/// All data sourced from WhyceID projections via ProjectionAdapter.
///
/// MUST NOT:
/// - Assign roles or modify permissions
/// - Compute access logic
/// - Call policy engine directly
/// - Modify any state
/// </summary>
public interface IAccessQueryService
{
    Task<IdentityAccessView?> GetIdentityAccessAsync(Guid identityId, CancellationToken cancellationToken = default);
}
