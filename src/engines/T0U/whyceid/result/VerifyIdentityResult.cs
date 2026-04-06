namespace Whyce.Engines.T0U.WhyceId.Result;

public sealed record VerifyIdentityResult(bool IsVerified, string VerificationHash, string VerificationMethod);
