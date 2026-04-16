using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Classifies an inbound event envelope into zero-or-more violation signals.
/// Implementations delegate the rule matching to WHYCEPOLICY (Rego package
/// <c>data.whyce.enforcement</c>); the abstraction isolates the runtime
/// detection worker from the concrete policy engine.
///
/// Evaluation must be:
///   • deterministic — the same envelope must always yield the same signals
///     for a given policy version;
///   • side-effect free — no aggregate mutation, no event emission, no
///     I/O beyond the policy engine round-trip;
///   • deny-by-default — when no rule matches, return an empty collection.
/// </summary>
public interface IEnforcementEventEvaluator
{
    Task<IReadOnlyList<ViolationSignal>> EvaluateAsync(
        IEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}
