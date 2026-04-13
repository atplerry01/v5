namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Provides the authenticated caller's identity from the current HTTP context.
/// Fail-closed: throws if no authenticated identity is available.
/// Used by SystemIntentDispatcher to populate CommandContext.ActorId and TenantId
/// from the validated JWT token instead of a hardcoded "system" fallback.
///
/// WP-1 (Security Binding Completion): This interface is the boundary seam
/// between HTTP authentication (platform layer) and command context creation
/// (runtime layer). The implementation lives in the host adapter layer and
/// reads from ClaimsPrincipal; the consumer (SystemIntentDispatcher) depends
/// only on this abstraction via shared contracts.
/// </summary>
public interface ICallerIdentityAccessor
{
    /// <summary>
    /// Returns the authenticated caller's actor ID (JWT "sub" claim).
    /// Throws <see cref="InvalidOperationException"/> if no authenticated identity exists.
    /// </summary>
    string GetActorId();

    /// <summary>
    /// Returns the authenticated caller's tenant ID (JWT "tenant" claim or X-Tenant-Id header).
    /// Falls back to "default" only when the claim is absent on an otherwise authenticated request.
    /// </summary>
    string GetTenantId();
}
