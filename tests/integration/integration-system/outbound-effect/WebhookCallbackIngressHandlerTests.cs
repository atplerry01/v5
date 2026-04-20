using System.Security.Cryptography;
using System.Text;
using Whycespace.Domain.IntegrationSystem.OutboundEffect;
using Whycespace.Engines.T2E.OutboundEffects.Lifecycle;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.OutboundEffects;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Whycespace.Tests.Integration.Setup;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.4 — `WebhookCallbackIngressHandler` covers the webhook-ingress
/// correlation rules: happy path dispatches Finalized; orphans translate
/// to deterministic outcome codes with explicit evidence;
/// correlated-but-unknown outcomes emit reconciliation-required.
/// </summary>
public sealed class WebhookCallbackIngressHandlerTests
{
    private const string ProviderId = "http-webhook";
    private const string SigningKey = "test-signing-key";
    private static readonly Guid EffectId = Guid.Parse("88888888-0000-0000-0000-000000000001");

    private static (InMemoryOutboundEffectQueueStore queue,
                    CapturingFabric fabric,
                    OutboundEffectFinalityService finality,
                    WebhookCallbackIngressHandler handler,
                    AdvanceableClock clock) NewHarness()
    {
        var queue = new InMemoryOutboundEffectQueueStore();
        var fabric = new CapturingFabric();
        var clock = new AdvanceableClock();
        var finality = new OutboundEffectFinalityService(
            queue,
            new OutboundEffectLifecycleEventFactory(new StubPayloadRegistry()),
            fabric, clock, new OutboundEffectsMeter());
        var verifier = new WebhookDeliverySignatureVerifier(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ProviderId] = SigningKey,
            });
        var handler = new WebhookCallbackIngressHandler(queue, finality, verifier);
        return (queue, fabric, finality, handler, clock);
    }

    private static string Sign(byte[] body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningKey));
        return "sha256=" + Convert.ToHexString(hmac.ComputeHash(body)).ToLowerInvariant();
    }

    private static async Task SeedAcknowledged(InMemoryOutboundEffectQueueStore queue, AdvanceableClock clock)
    {
        await queue.InsertAsync(new OutboundEffectQueueEntry
        {
            EffectId = EffectId,
            ProviderId = ProviderId,
            EffectType = "notification.send",
            IdempotencyKey = "k-1",
            Status = OutboundEffectQueueStatus.Acknowledged,
            AttemptCount = 1,
            MaxAttempts = 5,
            NextAttemptAt = clock.UtcNow,
            DispatchDeadline = clock.UtcNow.AddMinutes(5),
            FinalityDeadline = clock.UtcNow.AddMinutes(10),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
            Payload = new object(),
        });
    }

    [Fact]
    public async Task Happy_path_finalizes_on_correlated_webhook_callback()
    {
        var (queue, fabric, _, handler, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        var body = Encoding.UTF8.GetBytes("{\"status\":\"succeeded\"}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            EffectId, ProviderId, ProviderOperationId: "op-abc",
            SignatureHeader: Sign(body), Body: body,
            OutcomeCode: "Succeeded", EvidenceDigest: "digest-ok"));

        Assert.Equal(WebhookIngressOutcome.Finalized, outcome);
        Assert.Equal(OutboundEffectQueueStatus.Finalized,
            (await queue.GetAsync(EffectId))!.Status);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectFinalizedEvent);
    }

    [Fact]
    public async Task Orphan_effect_id_rejects_with_unknown_effect_outcome()
    {
        var (_, fabric, _, handler, _) = NewHarness();
        // No seeded row — the effect id is unknown.

        var body = Encoding.UTF8.GetBytes("{\"status\":\"succeeded\"}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            Guid.NewGuid(), ProviderId, "op-abc", Sign(body), body,
            "Succeeded", "digest-ok"));

        Assert.Equal(WebhookIngressOutcome.UnknownEffect, outcome);
        // No lifecycle events emitted for orphan callbacks.
        Assert.Empty(fabric.ProcessedEvents);
    }

    [Fact]
    public async Task Signature_mismatch_rejects_deterministically()
    {
        var (queue, fabric, _, handler, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        var body = Encoding.UTF8.GetBytes("{\"status\":\"succeeded\"}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            EffectId, ProviderId, "op-abc",
            SignatureHeader: "sha256=deadbeef", Body: body,
            OutcomeCode: "Succeeded", EvidenceDigest: "digest-ok"));

        Assert.Equal(WebhookIngressOutcome.SignatureInvalid, outcome);
        Assert.Empty(fabric.ProcessedEvents);
    }

    [Fact]
    public async Task Unknown_outcome_code_emits_reconciliation_required()
    {
        var (queue, fabric, _, handler, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);

        var body = Encoding.UTF8.GetBytes("{\"status\":\"weird\"}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            EffectId, ProviderId, "op-abc",
            Sign(body), body,
            OutcomeCode: "Mystery", EvidenceDigest: "digest"));

        Assert.Equal(WebhookIngressOutcome.Reconciled, outcome);
        Assert.Equal(OutboundEffectQueueStatus.ReconciliationRequired,
            (await queue.GetAsync(EffectId))!.Status);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciliationRequiredEvent r
            && r.Cause == nameof(OutboundReconciliationCause.ProviderDisagreement));
    }

    [Fact]
    public async Task Reconciliation_required_status_routes_webhook_to_reconcile_path()
    {
        var (queue, fabric, finality, handler, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);
        await finality.MarkReconciliationRequiredAsync(
            EffectId, OutboundReconciliationCause.FinalityTimeoutExpired, "expired", default);

        var body = Encoding.UTF8.GetBytes("{}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            EffectId, ProviderId, "op-abc", Sign(body), body,
            OutcomeCode: "Succeeded", EvidenceDigest: "late-arrival"));

        Assert.Equal(WebhookIngressOutcome.Reconciled, outcome);
        Assert.Equal(OutboundEffectQueueStatus.Reconciled,
            (await queue.GetAsync(EffectId))!.Status);
        Assert.Contains(fabric.ProcessedEvents, e => e is OutboundEffectReconciledEvent);
    }

    [Fact]
    public async Task Finalized_status_rejects_late_callback_as_invalid_status()
    {
        var (queue, fabric, finality, handler, clock) = NewHarness();
        await SeedAcknowledged(queue, clock);
        await finality.FinalizeAsync(EffectId, OutboundFinalityOutcome.Succeeded, "d", "Push", default);
        fabric.ProcessedEvents.Clear();

        var body = Encoding.UTF8.GetBytes("{}");
        var outcome = await handler.HandleAsync(new WebhookCallbackIngressRequest(
            EffectId, ProviderId, "op-abc", Sign(body), body,
            OutcomeCode: "Succeeded", EvidenceDigest: "d"));

        Assert.Equal(WebhookIngressOutcome.InvalidStatus, outcome);
        Assert.Empty(fabric.ProcessedEvents);
    }

    private sealed class CapturingFabric : IEventFabric
    {
        public List<object> ProcessedEvents { get; } = new();
        public Task ProcessAsync(
            IReadOnlyList<object> domainEvents, CommandContext context, CancellationToken ct = default)
        {
            ProcessedEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
        public Task ProcessAuditAsync(AuditEmission audit, CommandContext context, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class StubPayloadRegistry : IPayloadTypeRegistry
    {
        public void Register(Type type) { }
        public void Register<T>() { }
        public bool TryGetName(Type type, out string? name) { name = null; return false; }
        public Type Resolve(string typeName) => throw new NotSupportedException();
    }
}
