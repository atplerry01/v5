using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditLog;

public static class AuditLogErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuditLogId must not be null or empty.");

    public static DomainException IdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AuditLogId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
