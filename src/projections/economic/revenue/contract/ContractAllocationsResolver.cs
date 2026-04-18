using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

namespace Whycespace.Projections.Economic.Revenue.Contract;

/// <summary>
/// Phase 3.5 T3.5.2 — read-side implementation of
/// <see cref="IContractAllocationsResolver"/>. Reads the projected
/// <c>Parties</c> list from the contract read model and maps each
/// <c>ContractPartyShare</c> into a <see cref="DistributionAllocation"/>
/// (PartyId.ToString() → ParticipantId; SharePercentage → OwnershipPercentage).
///
/// Phase 3.6 T3.6.4 — fail-fast contract for backfill safety. The resolver
/// distinguishes three states and reacts differently:
///   1. Contract row not found              → return empty list (caller handles).
///   2. Contract row exists, no parties key → throw with explicit pointer to
///      the backfill script. This is the "Phase 3.6 backfill not run"
///      signal; silently returning empty would let TriggerDistributionStep
///      report a generic "zero allocations" failure that hides the real
///      cause (legacy contract row).
///   3. Contract row exists, parties array empty → throw. The contract
///      aggregate's invariant requires ≥ 2 parties, so an empty array is a
///      corrupted projection, not a valid state.
/// </summary>
public sealed class ContractAllocationsResolver : IContractAllocationsResolver
{
    // The query returns both the JSONB state AND a discrete `has_parties_key`
    // boolean so the resolver can distinguish "parties key absent" (backfill
    // gap) from "parties array empty" (corruption) — two failure modes that
    // both deserialize to an empty C# list and would otherwise be
    // indistinguishable in the C# layer.
    private const string Sql = @"
        SELECT state, (state ? 'parties') AS has_parties_key
        FROM projection_economic_revenue_contract.revenue_contract_read_model
        WHERE aggregate_id = @id
        LIMIT 1";

    private const string BackfillScriptPath =
        "infrastructure/data/postgres/projections/economic/revenue/contract/backfill/001_backfill_parties.sql";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly NpgsqlDataSource _dataSource;

    public ContractAllocationsResolver(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<IReadOnlyList<DistributionAllocation>> ResolveAsync(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(Sql, conn);
        cmd.Parameters.AddWithValue("id", contractId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return Array.Empty<DistributionAllocation>();

        var stateJson = reader.GetString(0);
        var hasPartiesKey = reader.GetBoolean(1);

        if (!hasPartiesKey)
            throw new InvalidOperationException(
                $"ContractAllocationsResolver: contract {contractId} exists in the projection but has no 'parties' field in its state. " +
                $"This indicates a pre-Phase-3.6 row not covered by the parties backfill. " +
                $"Run {BackfillScriptPath} (and the matching validation query) before driving this contract through the Phase 3 pipeline.");

        var model = JsonSerializer.Deserialize<ContractReadModel>(stateJson, Json);
        if (model is null)
            throw new InvalidOperationException(
                $"ContractAllocationsResolver: failed to deserialize contract {contractId} state JSON.");

        if (model.Parties.Count == 0)
            throw new InvalidOperationException(
                $"ContractAllocationsResolver: contract {contractId} has an empty 'parties' array. " +
                $"The contract aggregate's invariant requires at least 2 parties, so an empty projection is a corrupted row, not a valid state. " +
                $"Investigate the projection rebuild or the source RevenueContractCreatedEvent before proceeding.");

        var allocations = new List<DistributionAllocation>(model.Parties.Count);
        foreach (var party in model.Parties)
            allocations.Add(new DistributionAllocation(party.PartyId.ToString(), party.SharePercentage));

        return allocations;
    }
}
