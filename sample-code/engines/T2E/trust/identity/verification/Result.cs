namespace Whycespace.Engines.T2E.Trust.Identity.Verification;

public record VerificationResult(bool Success, string Message);
public sealed record VerificationDto(string VerificationId, string IdentityId, string VerificationType, string Status, int AttemptCount);
