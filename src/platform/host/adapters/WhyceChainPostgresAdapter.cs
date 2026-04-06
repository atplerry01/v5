using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// PostgreSQL-backed chain anchor. Persists immutable chain blocks to the
/// whyce_chain table. Each block references the previous block hash,
/// forming an append-only integrity chain.
/// </summary>
public sealed class WhyceChainPostgresAdapter : IChainAnchor
{
    private readonly string _connectionString;
    private readonly IClock _clock;

    public WhyceChainPostgresAdapter(string connectionString, IClock clock)
    {
        _connectionString = connectionString;
        _clock = clock;
    }

    public async Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash)
    {
        var previousBlockHash = await GetPreviousBlockHashAsync();
        var eventHash = ComputeEventHash(events);
        var blockId = ComputeBlockId(previousBlockHash, eventHash, decisionHash);
        var timestamp = _clock.UtcNow;

        var block = new ChainBlock(blockId, correlationId, eventHash, decisionHash, previousBlockHash, timestamp);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO whyce_chain (block_id, correlation_id, event_hash, decision_hash, previous_block_hash, timestamp)
            VALUES (@blockId, @corrId, @evtHash, @decHash, @prevHash, @ts)
            """,
            conn);

        cmd.Parameters.AddWithValue("blockId", block.BlockId);
        cmd.Parameters.AddWithValue("corrId", block.CorrelationId);
        cmd.Parameters.AddWithValue("evtHash", block.EventHash);
        cmd.Parameters.AddWithValue("decHash", block.DecisionHash);
        cmd.Parameters.AddWithValue("prevHash", block.PreviousBlockHash);
        cmd.Parameters.AddWithValue("ts", block.Timestamp);

        await cmd.ExecuteNonQueryAsync();

        return block;
    }

    private async Task<string> GetPreviousBlockHashAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT block_id FROM whyce_chain ORDER BY timestamp DESC LIMIT 1",
            conn);

        var result = await cmd.ExecuteScalarAsync();
        return result is Guid id ? id.ToString() : "genesis";
    }

    private static string ComputeEventHash(IReadOnlyList<object> events)
    {
        var payload = JsonSerializer.Serialize(events);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private static Guid ComputeBlockId(string previousHash, string eventHash, string decisionHash)
    {
        var seed = $"{previousHash}:{eventHash}:{decisionHash}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}
