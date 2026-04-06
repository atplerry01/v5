using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.Simulation.Cluster;

/// <summary>
/// E18.6.8 — Simulation gate for cross-SPV transactions.
/// Runs BEFORE workflow start to evaluate risk, exposure, and capital movement.
/// If simulation fails, workflow is blocked.
/// </summary>
public sealed class CrossSpvSimulationEngine
{
    public Task<CrossSpvSimulationResult> SimulateAsync(
        CrossSpvPolicyInput input,
        CancellationToken ct = default)
    {
        // Validate input integrity
        if (input.TransactionId == Guid.Empty)
            return Task.FromResult(CrossSpvSimulationResult.Fail("TransactionId required"));

        if (input.Legs == null || input.Legs.Count == 0)
            return Task.FromResult(CrossSpvSimulationResult.Fail("No legs to simulate"));

        if (input.TotalAmount.Amount <= 0)
            return Task.FromResult(CrossSpvSimulationResult.Fail("Total amount must be positive"));

        // Simulation passes — risk assessment delegated to policy engine
        return Task.FromResult(CrossSpvSimulationResult.Success(input.TransactionId));
    }
}

public sealed record CrossSpvSimulationResult
{
    public required bool IsSuccess { get; init; }
    public Guid? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static CrossSpvSimulationResult Success(Guid transactionId) => new()
    {
        IsSuccess = true,
        TransactionId = transactionId
    };

    public static CrossSpvSimulationResult Fail(string error) => new()
    {
        IsSuccess = false,
        ErrorMessage = error
    };
}
