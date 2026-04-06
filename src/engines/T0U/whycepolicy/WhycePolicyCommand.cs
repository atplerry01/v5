namespace Whyce.Engines.T0U.WhycePolicy;

public sealed record WhycePolicyCommand(
    string PolicyName,
    string IdentityId,
    string[] Roles,
    int TrustScore);
