namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Well-known context property keys for identity data stored in CommandContext.Properties.
/// Used by IdentityContextMiddleware to enrich and by IdentityExecutionContext to read.
/// </summary>
public static class IdentityContextKeys
{
    public const string IdentityId = "Identity.IdentityId";
    public const string IdentityType = "Identity.IdentityType";
    public const string IdentityStatus = "Identity.Status";
    public const string SessionId = "Identity.SessionId";
    public const string DeviceId = "Identity.DeviceId";
    public const string TrustLevel = "Identity.TrustLevel";
    public const string Roles = "Identity.Roles";
    public const string Permissions = "Identity.Permissions";
    public const string AuthenticationMethod = "Identity.AuthenticationMethod";
    public const string IsServiceIdentity = "Identity.IsServiceIdentity";
}
