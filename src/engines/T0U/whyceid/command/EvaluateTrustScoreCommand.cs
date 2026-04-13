namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record EvaluateTrustScoreCommand(string IdentityId, string[] Roles, string? DeviceId, bool IsVerified);
