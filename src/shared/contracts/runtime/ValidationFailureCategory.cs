namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Validation-stage rejection taxonomy. Consumed by <c>ValidationMiddleware</c>,
/// <c>ContextGuardMiddleware</c>, <c>ExecutionGuardMiddleware</c> to classify
/// what kind of validation failed.
///
/// See spec §5 Validation and Eligibility Features.
/// </summary>
public enum ValidationFailureCategory
{
    Unknown = 0,

    InputSchema,
    Semantic,
    CommandPrecondition,
    StateTransition,
    BusinessInvariant,
    DependencyReadiness,
    EnvironmentPosture,
    RestrictedOperation
}
