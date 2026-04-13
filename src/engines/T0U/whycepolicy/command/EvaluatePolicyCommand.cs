namespace Whycespace.Engines.T0U.WhycePolicy.Command;

public sealed record EvaluatePolicyCommand(
    string PolicyName,
    string IdentityId,
    string[] Roles,
    int TrustScore,
    string CommandType,
    string TenantId,
    string? ResourceId);
