using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

public static class ConfigurationAssignmentErrors
{
    public static DomainException AssignmentIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationAssignmentId must not be null or empty.");

    public static DomainException AssignmentIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"ConfigurationAssignmentId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException DefinitionIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationAssignment requires a non-empty definition id.");

    public static DomainException ScopeIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationAssignment requires a non-empty scope id.");

    public static DomainException ValueMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationAssignment value must not be null or empty.");

    public static DomainException AssignmentAlreadyRevoked() =>
        new DomainInvariantViolationException("ConfigurationAssignment is already revoked.");
}
