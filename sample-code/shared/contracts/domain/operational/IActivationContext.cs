namespace Whycespace.Shared.Contracts.Domain.Operational;

/// <summary>
/// System activation context for controlled execution modes (EX).
/// Gates the execution pipeline into: SIMULATION, SANDBOX, LIMITED_PRODUCTION, FULL_PRODUCTION.
/// All mode transitions are policy-gated.
/// </summary>
public interface IActivationContext
{
    string Mode { get; }
    bool IsSimulation { get; }
}
