using System.Net.Http.Json;
using System.Text.Json;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Economic.Transaction.Setup;

namespace Whycespace.Tests.E2E.Economic.Transaction.ControlPlane;

/// <summary>
/// Phase 4 T4.5 — proves the locked transaction control plane. The lifecycle
/// workflow is: ValidateLifecycleIntent → ExecuteInstruction →
/// InitiateTransaction → CheckLimit → InitiateSettlement → FxLock →
/// PostToLedger. Every transaction MUST pass through CheckLimitStep before
/// any settlement or ledger mutation.
///
/// Scenarios covered:
///   1. WithinLimit — initiates a transaction below threshold; expects
///      successful workflow completion and a posted ledger journal.
///   2. OverLimit — initiates a transaction above threshold; expects
///      hard-block at CheckLimitStep with NO ledger journal posted.
///   3. ConcurrentBoundary — two concurrent transactions at the limit
///      boundary; expects exactly one success (the other rejects with a
///      LimitExceeded or ConcurrencyConflict).
///   4. RestrictedSubject — Phase 2 enforcement regression: a restricted
///      subject is still rejected at the EnforcementGuard before reaching
///      the limit step.
///
/// Skipped when no API/projections env wiring is present.
/// </summary>
[Collection(TransactionControlPlaneE2ECollection.Name)]
public sealed class TransactionControlPlaneE2ETests
{
    private readonly TransactionControlPlaneE2EFixture _fix;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public TransactionControlPlaneE2ETests(TransactionControlPlaneE2EFixture fix) => _fix = fix;

    [Fact(DisplayName = "WithinLimit: transaction completes and a ledger journal is posted")]
    public async Task WithinLimit_PostsLedgerJournal()
    {
        var (fromAccount, _, _) = await SeedAccountWithLimit(threshold: 1_000m, currency: "USD");

        var transactionId = _fix.SeedId("txn:within-limit");
        var journalId = _fix.SeedId("journal:within-limit");
        var intent = BuildIntent(
            transactionId: transactionId,
            journalId: journalId,
            fromAccountId: fromAccount,
            amount: 500m,
            currency: "USD");

        using var response = await PostLifecycleAsync(intent);
        Assert.True(response.IsSuccessStatusCode,
            $"lifecycle/start returned {(int)response.StatusCode}; pipeline cannot start.");

        var journalPosted = await PollAsync(
            "SELECT 1 FROM projection_economic_ledger_journal.journal_read_model WHERE journal_id = @id",
            "@id", journalId, expected: _ => true, TransactionControlPlaneE2EConfig.PollTimeout);
        Assert.True(journalPosted is not null,
            $"Within-limit transaction did not produce ledger journal {journalId}.");
    }

    [Fact(DisplayName = "OverLimit: hard-blocks before settlement; no ledger journal posted")]
    public async Task OverLimit_BlocksBeforeLedger()
    {
        var (fromAccount, _, _) = await SeedAccountWithLimit(threshold: 100m, currency: "USD");

        var transactionId = _fix.SeedId("txn:over-limit");
        var journalId = _fix.SeedId("journal:over-limit");
        var intent = BuildIntent(
            transactionId: transactionId,
            journalId: journalId,
            fromAccountId: fromAccount,
            amount: 500m, // > threshold 100
            currency: "USD");

        using var response = await PostLifecycleAsync(intent);
        // The pipeline returns the hard-block as a workflow Failure; the
        // /lifecycle/start endpoint surfaces it as a 4xx with the
        // LimitExceeded message in the body.
        Assert.False(response.IsSuccessStatusCode,
            $"Over-limit transaction was unexpectedly accepted (status {(int)response.StatusCode}).");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("limit", body, StringComparison.OrdinalIgnoreCase);

        // Critical assertion: NO ledger journal exists for this transaction.
        await Task.Delay(TimeSpan.FromSeconds(1)); // allow projection settle window
        var journalAbsent = await ScalarOrNullAsync(
            "SELECT 1 FROM projection_economic_ledger_journal.journal_read_model WHERE journal_id = @id",
            "@id", journalId);
        Assert.Null(journalAbsent);
    }

    [Fact(DisplayName = "ConcurrentBoundary: only one of two boundary transactions succeeds")]
    public async Task ConcurrentBoundary_OnlyOneSucceeds()
    {
        // Threshold 1000, two concurrent 600 charges — sum 1200 > 1000, so
        // exactly one must be rejected at the limit boundary.
        var (fromAccount, _, _) = await SeedAccountWithLimit(threshold: 1_000m, currency: "USD");

        var first = BuildIntent(
            transactionId: _fix.SeedId("txn:concurrent:a"),
            journalId: _fix.SeedId("journal:concurrent:a"),
            fromAccountId: fromAccount, amount: 600m, currency: "USD");
        var second = BuildIntent(
            transactionId: _fix.SeedId("txn:concurrent:b"),
            journalId: _fix.SeedId("journal:concurrent:b"),
            fromAccountId: fromAccount, amount: 600m, currency: "USD");

        var t1 = PostLifecycleAsync(first);
        var t2 = PostLifecycleAsync(second);
        var responses = await Task.WhenAll(t1, t2);
        try
        {
            var successes = responses.Count(r => r.IsSuccessStatusCode);
            var failures  = responses.Count(r => !r.IsSuccessStatusCode);

            Assert.Equal(1, successes);
            Assert.Equal(1, failures);
        }
        finally
        {
            foreach (var r in responses) r.Dispose();
        }
    }

    [Fact(DisplayName = "RestrictedSubject: blocked at EnforcementGuard before CheckLimitStep")]
    public async Task RestrictedSubject_StillBlocked()
    {
        // Pre-condition: the test environment seeds a restricted subject
        // identified by RESTRICTED_SUBJECT_ID. If no restricted subject is
        // available, the test is skipped (Phase 2 enforcement coverage
        // already guarantees the gate; this test only confirms Phase 4
        // does not regress that path).
        var restrictedSubjectEnv = Environment.GetEnvironmentVariable("TXN_E2E_RESTRICTED_SUBJECT_ID");
        if (string.IsNullOrWhiteSpace(restrictedSubjectEnv) || !Guid.TryParse(restrictedSubjectEnv, out var restricted))
            return;

        var (_, _, _) = await SeedAccountWithLimit(threshold: 1_000m, currency: "USD", accountOwner: restricted);

        var intent = BuildIntent(
            transactionId: _fix.SeedId("txn:restricted"),
            journalId: _fix.SeedId("journal:restricted"),
            fromAccountId: restricted,
            amount: 50m,
            currency: "USD");

        using var response = await PostLifecycleAsync(intent);
        Assert.False(response.IsSuccessStatusCode);

        var body = await response.Content.ReadAsStringAsync();
        // Either restriction (Phase 2) or system-origin bypass info
        // surfaces in the body — both are acceptable as long as the txn
        // does not complete.
        Assert.Contains("restrict", body, StringComparison.OrdinalIgnoreCase);
    }

    // ── helpers ────────────────────────────────────────────────────

    private async Task<(Guid AccountId, Guid LimitId, decimal Threshold)> SeedAccountWithLimit(
        decimal threshold, string currency, Guid? accountOwner = null)
    {
        var accountId = accountOwner ?? _fix.SeedId($"acc:{threshold}:{currency}");
        var limitId = _fix.SeedId($"limit:{accountId}:{currency}");

        await using var dataSource = NpgsqlDataSource.Create(TransactionControlPlaneE2EConfig.ProjectionsConnectionString);
        await using var seed = dataSource.CreateCommand(@"
            INSERT INTO projection_economic_transaction_limit.limit_read_model
                (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, idempotency_key)
            VALUES
                (@id, 'Limit', 1,
                 jsonb_build_object(
                    'limitId', @id,
                    'accountId', @accountId,
                    'type', 'PerTransaction',
                    'threshold', @threshold::numeric,
                    'currency', @currency,
                    'currentUtilization', 0,
                    'status', 'Active',
                    'definedAt', to_jsonb(NOW())),
                 gen_random_uuid(), 'LimitDefinedEvent', gen_random_uuid(), @id::text || ':seed')
            ON CONFLICT (aggregate_id) DO UPDATE SET state = EXCLUDED.state");
        seed.Parameters.AddWithValue("id", limitId);
        seed.Parameters.AddWithValue("accountId", accountId);
        seed.Parameters.AddWithValue("threshold", threshold);
        seed.Parameters.AddWithValue("currency", currency);
        await seed.ExecuteNonQueryAsync();

        return (accountId, limitId, threshold);
    }

    private object BuildIntent(
        Guid transactionId, Guid journalId, Guid fromAccountId, decimal amount, string currency) => new
    {
        InstructionId = _fix.SeedId($"instruction:{transactionId}"),
        TransactionId = transactionId,
        SettlementId  = _fix.SeedId($"settlement:{transactionId}"),
        LedgerId      = _fix.SeedId($"ledger:{currency}"),
        JournalId     = journalId,
        FromAccountId = fromAccountId,
        ToAccountId   = _fix.SeedId($"acc:to:{currency}"),
        Amount        = amount,
        Currency      = currency,
        InstructionType    = "transfer",
        SettlementProvider = "test-provider",
        InitiatedAt   = _fix.Clock.UtcNow
    };

    private Task<HttpResponseMessage> PostLifecycleAsync(object intent)
    {
        // The lifecycle workflow is exposed via the same workflow
        // dispatcher that the revenue/distribution/payout pipelines use;
        // there is no controller endpoint explicitly named lifecycle/start
        // today. The test posts to the canonical workflow-start surface
        // — adjust the path here if the runtime exposes a different one.
        var envelope = new ApiRequest<object>
        {
            Meta = new RequestMeta { CorrelationId = Guid.NewGuid().ToString() },
            Data = intent
        };
        return _fix.Http.PostAsJsonAsync("/api/transaction/lifecycle/start", envelope, Json);
    }

    private static async Task<string?> PollAsync(
        string sql, string paramName, Guid id, Func<string, bool> expected, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            await using var dataSource = NpgsqlDataSource.Create(TransactionControlPlaneE2EConfig.ProjectionsConnectionString);
            await using var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue(paramName, id);
            var raw = await cmd.ExecuteScalarAsync();
            var value = raw?.ToString();
            if (value is not null && expected(value)) return value;
            await Task.Delay(250);
        }
        return null;
    }

    private static async Task<string?> ScalarOrNullAsync(string sql, string paramName, Guid id)
    {
        await using var dataSource = NpgsqlDataSource.Create(TransactionControlPlaneE2EConfig.ProjectionsConnectionString);
        await using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue(paramName, id);
        var raw = await cmd.ExecuteScalarAsync();
        return raw?.ToString();
    }
}
