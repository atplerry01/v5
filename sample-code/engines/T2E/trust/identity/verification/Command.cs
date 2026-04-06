namespace Whycespace.Engines.T2E.Trust.Identity.Verification;

public record VerificationCommand(string Action, string EntityId, object Payload);
public sealed record CreateVerificationCommand(string IdentityId, string VerificationType, string Method, int MaxAttempts) : VerificationCommand("Create", IdentityId, null!);
public sealed record AddVerificationAttemptCommand(string VerificationId, string Evidence) : VerificationCommand("AddAttempt", VerificationId, null!);
public sealed record CompleteVerificationCommand(string VerificationId) : VerificationCommand("Complete", VerificationId, null!);
public sealed record FailVerificationCommand(string VerificationId, string Reason) : VerificationCommand("Fail", VerificationId, null!);
public sealed record ExpireVerificationCommand(string VerificationId) : VerificationCommand("Expire", VerificationId, null!);
