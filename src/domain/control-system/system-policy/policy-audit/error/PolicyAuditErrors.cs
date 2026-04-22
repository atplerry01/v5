using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public static class PolicyAuditErrors
{
    public static DomainException AuditIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyAuditId must not be null or empty.");

    public static DomainException AuditIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyAuditId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException PolicyIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyAudit requires a non-empty policyId.");

    public static DomainException ActorIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyAudit requires a non-empty actorId.");

    public static DomainException ReviewReasonMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyAudit review reason must not be empty.");

    public static DomainException AuditEntryAlreadyReviewed() =>
        new DomainInvariantViolationException("PolicyAudit entry has already been reviewed.");
}
