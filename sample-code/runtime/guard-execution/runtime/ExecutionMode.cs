namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Execution mode for the enforcement pipeline.
/// Live: full guard + policy + chain evaluation.
/// Replay: bypass guard/policy, reuse stored hashes — deterministic replay.
/// </summary>
public enum ExecutionMode
{
    Live = 0,
    Replay = 1
}
