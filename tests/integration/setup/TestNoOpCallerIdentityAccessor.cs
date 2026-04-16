using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// In-process test stub for <see cref="ICallerIdentityAccessor"/>. The
/// integration TestHost runs without an HTTP context, so this stub returns
/// fixed values that match the synthetic CommandContext the host builds
/// (ActorId="test-actor", TenantId="test-tenant") and an empty roles array
/// so PolicyMiddleware falls through to WhyceIdEngine's safe default.
/// </summary>
internal sealed class TestNoOpCallerIdentityAccessor : ICallerIdentityAccessor
{
    public string GetActorId() => "test-actor";
    public string GetTenantId() => "test-tenant";
    public string[] GetRoles() => Array.Empty<string>();
    public IReadOnlyDictionary<string, object> GetSubjectAttributes() =>
        new Dictionary<string, object>(StringComparer.Ordinal);
}
