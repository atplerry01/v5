using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;

namespace Whycespace.Projections.Economic.Revenue.Contract;

/// <summary>
/// Phase 3.5 T3.5.2 — read-side implementation of <see cref="IContractStatusGate"/>.
/// Reads the canonical contract projection
/// (<c>projection_economic_revenue_contract.revenue_contract_read_model</c>)
/// and returns an <see cref="ContractStatusGateResult"/> based on the JSONB
/// <c>state.status</c> field. CQRS-clean: never touches the event store, never
/// loads the write-side aggregate.
/// </summary>
public sealed class ContractStatusGate : IContractStatusGate
{
    private const string Sql = @"
        SELECT state
        FROM projection_economic_revenue_contract.revenue_contract_read_model
        WHERE aggregate_id = @id
        LIMIT 1";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly NpgsqlDataSource _dataSource;

    public ContractStatusGate(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<ContractStatusGateResult> CheckAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var model = await LoadAsync(contractId, cancellationToken);
        if (model is null)
            return ContractStatusGateResult.NotFound(contractId);

        return string.Equals(model.Status, "Active", StringComparison.Ordinal)
            ? ContractStatusGateResult.Active()
            : ContractStatusGateResult.NotActive(contractId, model.Status);
    }

    private async Task<ContractReadModel?> LoadAsync(Guid contractId, CancellationToken ct)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(Sql, conn);
        cmd.Parameters.AddWithValue("id", contractId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var json = reader.GetString(0);
        return JsonSerializer.Deserialize<ContractReadModel>(json, Json);
    }
}
