namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Autonomy context for policy-bounded autonomous decisions (E18.5).
/// Resolved from runtime configuration and policy state.
/// Autonomy MUST be explicitly enabled and environment-gated.
/// </summary>
public interface IAutonomyContext
{
    bool IsAutonomyEnabled { get; }
    string Environment { get; }
}
