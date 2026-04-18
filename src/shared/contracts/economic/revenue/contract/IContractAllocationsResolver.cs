using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

/// <summary>
/// Resolves the canonical participant share rules for an active contract
/// into <see cref="DistributionAllocation"/> records consumable by
/// <c>CreateDistributionCommand</c>. Read-side port (Phase 3 T3.1) —
/// implementations look up the contract projection; this is NOT a
/// write-side aggregate load.
/// </summary>
public interface IContractAllocationsResolver
{
    Task<IReadOnlyList<DistributionAllocation>> ResolveAsync(
        Guid contractId,
        CancellationToken cancellationToken = default);
}
