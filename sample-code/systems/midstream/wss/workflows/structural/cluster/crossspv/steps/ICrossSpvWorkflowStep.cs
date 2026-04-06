namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// Contract for cross-SPV workflow steps.
/// Systems layer defines the step interface; T1M executes via this contract.
/// </summary>
public interface ICrossSpvWorkflowStep
{
    Task<CrossSpvStepResult> ExecuteAsync(
        CrossSpvStepCommand command,
        CancellationToken ct = default);
}

public abstract record CrossSpvStepCommand(Guid TransactionId, string CorrelationId);

public sealed record PrepareCrossSpvStepCommand(
    Guid TransactionId,
    Guid RootSpvId,
    string CorrelationId
) : CrossSpvStepCommand(TransactionId, CorrelationId);

public sealed record ExecuteCrossSpvLegsStepCommand(
    Guid TransactionId,
    string CorrelationId
) : CrossSpvStepCommand(TransactionId, CorrelationId);

public sealed record CommitCrossSpvStepCommand(
    Guid TransactionId,
    string CorrelationId
) : CrossSpvStepCommand(TransactionId, CorrelationId);

public sealed record FailCrossSpvStepCommand(
    Guid TransactionId,
    string Reason,
    string CorrelationId
) : CrossSpvStepCommand(TransactionId, CorrelationId);

public sealed record CrossSpvStepResult(
    Guid TransactionId,
    string StepName,
    bool Success,
    string? FailureReason = null)
{
    public static CrossSpvStepResult Ok(Guid transactionId, string stepName)
        => new(transactionId, stepName, true);

    public static CrossSpvStepResult Fail(Guid transactionId, string stepName, string reason)
        => new(transactionId, stepName, false, reason);
}
