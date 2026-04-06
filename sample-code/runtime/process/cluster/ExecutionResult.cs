namespace Whycespace.Runtime.Process.Cluster;

/// <summary>
/// H7 — Tracks the result of each step in a cross-SPV execution pipeline.
/// Used by the process manager to determine compensation scope on failure.
/// </summary>
public sealed record ExecutionResult
{
    public required Guid ReferenceId { get; init; }
    public required ExecutionStage Stage { get; init; }
    public required bool Success { get; init; }
    public required int LegIndex { get; init; }
    public string? ErrorMessage { get; init; }

    public static ExecutionResult Ok(Guid referenceId, ExecutionStage stage, int legIndex) => new()
    {
        ReferenceId = referenceId,
        Stage = stage,
        Success = true,
        LegIndex = legIndex
    };

    public static ExecutionResult Fail(Guid referenceId, ExecutionStage stage, int legIndex, string error) => new()
    {
        ReferenceId = referenceId,
        Stage = stage,
        Success = false,
        LegIndex = legIndex,
        ErrorMessage = error
    };
}

public enum ExecutionStage
{
    Ledger,
    Settlement,
    Revenue,
    Distribution
}
