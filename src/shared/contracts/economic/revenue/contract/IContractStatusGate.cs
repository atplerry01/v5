namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

/// <summary>
/// Read-side gate for the canonical economic pipeline (Phase 3 T3.6).
/// Workflow steps consult this before allowing revenue / pricing / distribution
/// / payout work to proceed. Implementations resolve status from the contract
/// projection (read model) — this is a read-side check, NOT a write-side
/// aggregate load (preserves CQRS GE-05).
///
/// Returning a <see cref="ContractStatusGateResult"/> rather than a bool keeps
/// the gate's reason string deterministic for audit / observability.
/// </summary>
public interface IContractStatusGate
{
    Task<ContractStatusGateResult> CheckAsync(Guid contractId, CancellationToken cancellationToken = default);
}

public sealed record ContractStatusGateResult(bool IsActive, string Reason)
{
    public static ContractStatusGateResult Active() =>
        new(true, "Contract.Status == Active");

    public static ContractStatusGateResult NotFound(Guid contractId) =>
        new(false, $"Contract {contractId} not found.");

    public static ContractStatusGateResult NotActive(Guid contractId, string status) =>
        new(false, $"Contract {contractId} is not Active (status: {status}).");
}
