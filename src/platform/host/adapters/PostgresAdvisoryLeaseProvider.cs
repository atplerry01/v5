using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// R2.A.C.1 / R-LEASE-POSTGRES-01 — Postgres advisory-lock implementation
/// of <see cref="IDistributedLeaseProvider"/>. Session-level advisory
/// locks are held for the lifetime of a dedicated
/// <see cref="NpgsqlConnection"/> per acquired lease — crash-safe by
/// construction per R-LEASE-CRASH-SAFE-01.
///
/// String lease keys are hashed to <c>bigint</c> (first 8 bytes of
/// SHA256) for <c>pg_try_advisory_lock</c>. Collision probability is
/// negligible at current scale (see R-LEASE-POSTGRES-01).
/// </summary>
public sealed class PostgresAdvisoryLeaseProvider : IDistributedLeaseProvider
{
    private readonly EventStoreDataSource _dataSource;
    private readonly IClock _clock;

    public PostgresAdvisoryLeaseProvider(EventStoreDataSource dataSource, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(clock);
        _dataSource = dataSource;
        _clock = clock;
    }

    public async Task<ILease?> TryAcquireAsync(
        string leaseKey,
        string holder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(leaseKey))
            throw new ArgumentException("leaseKey is required.", nameof(leaseKey));
        if (string.IsNullOrWhiteSpace(holder))
            throw new ArgumentException("holder is required.", nameof(holder));

        var keyHash = HashKeyToBigint(leaseKey);

        // Dedicated connection: session lifetime == lease lifetime.
        // The connection is NOT returned to the pool until DisposeAsync
        // runs — this is the canonical advisory-lock pattern.
        //
        // R2.A.D.3c / R-POSTGRES-POOL-BREAKER-OPEN-SEMANTICS-01: pool
        // breaker Open → null (busy signal). Lease callers already
        // treat null as "unavailable, try later"; this matches the
        // "busy signal" row in the posture table.
        NpgsqlConnection conn;
        try
        {
            conn = await _dataSource.OpenAsync(cancellationToken);
        }
        catch (CircuitBreakerOpenException)
        {
            return null;
        }

        try
        {
            await using var cmd = new NpgsqlCommand(
                "SELECT pg_try_advisory_lock(@key_hash)",
                conn);
            cmd.Parameters.AddWithValue("key_hash", keyHash);

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            var acquired = result is bool b && b;

            if (!acquired)
            {
                // Busy — return the connection to the pool before signalling null.
                await conn.DisposeAsync();
                return null;
            }

            // Retain the connection. PostgresAdvisoryLease owns it now and
            // will close it on DisposeAsync (releasing the session lock).
            return new PostgresAdvisoryLease(
                conn, leaseKey, holder, keyHash, _clock.UtcNow);
        }
        catch
        {
            // Any failure between pool-check-out and successful acquire
            // releases the connection back. Callers see the exception.
            await conn.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// R-LEASE-POSTGRES-01 — SHA256-prefix key hash. Deterministic across
    /// processes and releases; same input string always yields the same
    /// <c>bigint</c>. Caller-owned namespacing handles collision avoidance.
    /// </summary>
    internal static long HashKeyToBigint(string leaseKey)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(leaseKey), digest);

        long value = 0;
        for (int i = 0; i < 8; i++)
            value = (value << 8) | digest[i];
        return value;
    }

    private sealed class PostgresAdvisoryLease : ILease
    {
        private readonly NpgsqlConnection _connection;
        private readonly long _keyHash;
        private int _disposed;

        public string Key { get; }
        public string Holder { get; }
        public DateTimeOffset AcquiredAt { get; }

        public PostgresAdvisoryLease(
            NpgsqlConnection connection,
            string key,
            string holder,
            long keyHash,
            DateTimeOffset acquiredAt)
        {
            _connection = connection;
            _keyHash = keyHash;
            Key = key;
            Holder = holder;
            AcquiredAt = acquiredAt;
        }

        public async ValueTask DisposeAsync()
        {
            // Idempotent: double-dispose is a no-op.
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            try
            {
                // Best-effort explicit unlock. If this fails, closing the
                // connection below still releases the session-level lock
                // by construction — R-LEASE-CRASH-SAFE-01 holds.
                await using var cmd = new NpgsqlCommand(
                    "SELECT pg_advisory_unlock(@key_hash)",
                    _connection);
                cmd.Parameters.AddWithValue("key_hash", _keyHash);
                await cmd.ExecuteScalarAsync();
            }
            catch
            {
                // Swallow — the DisposeAsync of the connection below is
                // the authoritative release path.
            }
            finally
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
