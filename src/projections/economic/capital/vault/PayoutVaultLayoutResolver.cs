using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;

namespace Whycespace.Projections.Economic.Capital.Vault;

/// <summary>
/// Phase 3.5 T3.5.2 — read-side implementation of
/// <see cref="IPayoutVaultLayoutResolver"/>. Resolves the SPV vault id and
/// per-participant vault ids by consulting the capital vault projection
/// (<c>projection_economic_capital_vault.capital_vault_read_model</c>) keyed
/// on <c>OwnerId</c>. The Phase 3 T1M pipeline (TriggerPayoutStep) passes
/// participant ids as Guid strings — this resolver parses them and matches
/// against the projected <c>OwnerId</c>.
///
/// Currency selection: the resolver returns the first vault row found per
/// owner. The Phase 3 ledger journal step assumes a single payout currency
/// per SPV (USD by default); refine with a currency parameter if the
/// pipeline ever needs multi-currency payouts within a single distribution.
/// </summary>
public sealed class PayoutVaultLayoutResolver : IPayoutVaultLayoutResolver
{
    private const string SqlByOwner = @"
        SELECT state
        FROM projection_economic_capital_vault.capital_vault_read_model
        WHERE (state->>'ownerId')::uuid = @ownerId
        LIMIT 1";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly NpgsqlDataSource _dataSource;

    public PayoutVaultLayoutResolver(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<PayoutVaultLayout> ResolveAsync(
        string spvId,
        IReadOnlyCollection<string> participantIds,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(spvId, out var spvOwnerId))
            throw new InvalidOperationException(
                $"PayoutVaultLayoutResolver: SpvId '{spvId}' is not a Guid string. The economic.revenue pipeline requires Guid-shaped subject identifiers for vault resolution.");

        var spvVaultId = await ResolveOwnerVaultAsync(spvOwnerId, cancellationToken);
        if (spvVaultId == Guid.Empty)
            throw new InvalidOperationException(
                $"PayoutVaultLayoutResolver: no vault found for SPV owner {spvOwnerId}.");

        var participantVaults = new Dictionary<string, Guid>(participantIds.Count, StringComparer.Ordinal);
        foreach (var participantId in participantIds)
        {
            if (!Guid.TryParse(participantId, out var participantOwnerId))
                throw new InvalidOperationException(
                    $"PayoutVaultLayoutResolver: ParticipantId '{participantId}' is not a Guid string.");

            var vaultId = await ResolveOwnerVaultAsync(participantOwnerId, cancellationToken);
            if (vaultId == Guid.Empty)
                throw new InvalidOperationException(
                    $"PayoutVaultLayoutResolver: no vault found for participant owner {participantOwnerId}.");

            participantVaults[participantId] = vaultId;
        }

        return new PayoutVaultLayout(spvVaultId, participantVaults);
    }

    private async Task<Guid> ResolveOwnerVaultAsync(Guid ownerId, CancellationToken ct)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(SqlByOwner, conn);
        cmd.Parameters.AddWithValue("ownerId", ownerId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return Guid.Empty;

        var json = reader.GetString(0);
        var model = JsonSerializer.Deserialize<CapitalVaultReadModel>(json, Json);
        return model?.VaultId ?? Guid.Empty;
    }
}
