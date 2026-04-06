namespace Whyce.Engines.T0U.WhyceId;

public sealed record WhyceIdentity(
    string IdentityId,
    bool IsAuthenticated,
    bool IsVerified,
    string[] Roles,
    int TrustScore);
