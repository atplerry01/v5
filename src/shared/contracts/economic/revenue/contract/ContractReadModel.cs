namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

public sealed record ContractReadModel
{
    public Guid ContractId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset TermStart { get; init; }
    public DateTimeOffset TermEnd { get; init; }
    public int PartyCount { get; init; }

    /// <summary>
    /// Phase 3.5 T3.5.2 — projection of the contract's share rules so the
    /// Phase 3 <c>IContractAllocationsResolver</c> can derive distribution
    /// allocations from the read side without reaching back into the event
    /// store. Stored inside the JSONB <c>state</c> column; no DDL change.
    /// </summary>
    public IReadOnlyList<ContractPartyShare> Parties { get; init; } = Array.Empty<ContractPartyShare>();

    public string? TerminationReason { get; init; }
    public DateTimeOffset? TerminatedAt { get; init; }
}

public sealed record ContractPartyShare(Guid PartyId, decimal SharePercentage);
