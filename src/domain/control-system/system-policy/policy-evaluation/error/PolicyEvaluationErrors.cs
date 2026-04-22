using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

public static class PolicyEvaluationErrors
{
    public static DomainException EvaluationIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEvaluationId must not be null or empty.");

    public static DomainException EvaluationIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyEvaluationId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException PolicyIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEvaluation requires a non-empty policyId.");

    public static DomainException ActorIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEvaluation requires a non-empty actorId.");

    public static DomainException ActionMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEvaluation requires a non-empty action.");

    public static DomainException VerdictAlreadyIssued() =>
        new DomainInvariantViolationException("PolicyEvaluation verdict has already been issued.");

    public static DomainException DecisionHashMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEvaluation decision hash must not be empty.");
}
