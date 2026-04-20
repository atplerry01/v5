namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R1 §1 — long-running operation marker. Commands decorated with this
/// attribute are routed by <c>RuntimeCommandDispatcher</c> onto the async
/// / workflow-admission path instead of the synchronous request/response
/// path. Short-running commands (the default) complete inline.
///
/// Declaring this attribute is contract-only in R1 — the dispatcher's
/// routing consumption arrives in a later batch. Presence of the attribute
/// today is an advisory signal for the workflow admission gate and for
/// operator tooling.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
public sealed class LongRunningOperationAttribute : Attribute
{
    /// <summary>
    /// Expected execution budget in seconds. The workflow admission gate
    /// may use this to size per-command queue depth and to surface SLA
    /// violations. Default 0 = unspecified.
    /// </summary>
    public int ExpectedDurationSeconds { get; init; }

    /// <summary>
    /// When true, the command is expected to checkpoint progress via
    /// workflow suspend/resume (R3). When false, the command runs to
    /// completion in a single admitted lease.
    /// </summary>
    public bool SupportsSuspendResume { get; init; }
}
