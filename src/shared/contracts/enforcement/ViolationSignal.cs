namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// A single violation signal emitted by an enforcement event evaluator in
/// response to an incoming source event. One source event may produce zero,
/// one, or many signals — each signal becomes a separate DetectViolation
/// dispatch.
///
/// Severity + RecommendedAction must satisfy the aggregate invariant
/// defined in <c>ViolationAggregate.IsValidCombination</c>; a signal that
/// fails that invariant is rejected at command handling time.
/// </summary>
public sealed record ViolationSignal(
    Guid RuleId,
    Guid SourceReference,
    string Reason,
    string Severity,
    string RecommendedAction);
