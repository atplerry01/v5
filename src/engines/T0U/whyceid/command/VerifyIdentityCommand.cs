namespace Whyce.Engines.T0U.WhyceId.Command;

public sealed record VerifyIdentityCommand(string IdentityId, string VerificationMethod, string VerificationPayload);
