namespace Whycespace.Shared.Contracts.Identity;

/// <summary>
/// Identity context for policy decision binding (E5).
/// Resolved from request headers/tokens by the runtime.
/// Carried through the pipeline to enrich policy decisions and chain anchoring.
/// </summary>
public interface IIdentityContext
{
    string SubjectId { get; }
    string? SessionId { get; }
    string? DeviceId { get; }
    string[] Roles { get; }
    double TrustScore { get; }
    bool IsVerified { get; }
}
