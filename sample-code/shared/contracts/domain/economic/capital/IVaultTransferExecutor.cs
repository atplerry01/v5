namespace Whycespace.Shared.Contracts.Domain.Economic.Capital;

/// <summary>
/// Executes vault transfers as part of the distribution pipeline.
/// Distribution MUST execute vault transfers BEFORE finalizing the operation.
/// Implementations live in runtime; injected into DistributionDomainService.
///
/// E17.H4: Vault execution guarantee — no distribution without confirmed vault transfer.
/// </summary>
public interface IVaultTransferExecutor
{
    Task<VaultTransferResult> ExecuteTransferAsync(
        VaultTransferRequest transfer,
        DomainExecutionContext context,
        CancellationToken ct = default);
}

/// <summary>
/// Transfer request DTO — shared contract between distribution and vault.
/// Maps 1:1 from domain VaultTransfer entity.
/// </summary>
public sealed record VaultTransferRequest
{
    public required Guid TransferId { get; init; }
    public required Guid RecipientId { get; init; }
    public required decimal Amount { get; init; }
    public required string CurrencyCode { get; init; }
}

public sealed record VaultTransferResult
{
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? TransferId { get; init; }

    public static VaultTransferResult Ok(Guid transferId)
        => new() { Success = true, TransferId = transferId };

    public static VaultTransferResult Fail(string error)
        => new() { Success = false, ErrorMessage = error };
}
