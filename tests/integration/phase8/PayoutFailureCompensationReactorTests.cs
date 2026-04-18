using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase8;

/// <summary>
/// Phase 8 B7 — end-to-end validation of the B3 payout-failure →
/// compensation saga reactor
/// (<see cref="PayoutFailureCompensationIntegrationHandler"/>).
///
/// Exercises the handler with a synthetic
/// <see cref="PayoutFailedEventSchema"/> envelope and an
/// NSubstitute-mocked <see cref="IWorkflowDispatcher"/> so the
/// event-to-intent mapping, two-layer idempotency, and failure-release
/// semantics are observed without a real Kafka broker or workflow
/// engine.
/// </summary>
public sealed class PayoutFailureCompensationReactorTests
{
    private static RawTestEnvelope BuildFailedEnvelope(
        Guid payoutId,
        Guid distributionId,
        string reason,
        Guid? eventId = null) =>
        new()
        {
            EventId = eventId ?? Guid.Parse("33333333-3333-3333-3333-333333333333"),
            AggregateId = payoutId,
            CorrelationId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            EventType = nameof(PayoutFailedEventSchema),
            Payload = new PayoutFailedEventSchema(
                AggregateId: payoutId,
                DistributionId: distributionId,
                Reason: reason,
                FailedAt: new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero))
        };

    [Fact]
    public async Task PayoutFailedEvent_StartsCompensationWorkflow_WithCorrelatedIntent()
    {
        var workflowDispatcher = Substitute.For<IWorkflowDispatcher>();
        workflowDispatcher.StartWorkflowAsync(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<DomainRoute>())
            .Returns(Task.FromResult(WorkflowResult.Success()));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new PayoutFailureCompensationIntegrationHandler(
            workflowDispatcher, idempotency);

        var payoutId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000101");
        var distributionId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000102");
        var envelope = BuildFailedEnvelope(payoutId, distributionId, "gateway_timeout");

        await handler.HandleAsync(envelope);

        await workflowDispatcher.Received(1).StartWorkflowAsync(
            PayoutCompensationWorkflowNames.Compensate,
            Arg.Is<PayoutCompensationIntent>(i =>
                i.PayoutId == payoutId
                && i.DistributionId == distributionId
                && i.Reason == "gateway_timeout"),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "revenue"
                && r.Domain == "payout"));
    }

    [Fact]
    public async Task NonPayoutFailedPayload_IsObservationOnly_NoWorkflowStarted()
    {
        var workflowDispatcher = Substitute.For<IWorkflowDispatcher>();
        var idempotency = new InMemoryIdempotencyStore();
        var handler = new PayoutFailureCompensationIntegrationHandler(
            workflowDispatcher, idempotency);

        // PayoutCompensationRequestedEventSchema is another payload on the
        // same topic that the reactor must ignore.
        var envelope = new RawTestEnvelope
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            Payload = new PayoutCompensationRequestedEventSchema(
                AggregateId: Guid.NewGuid(),
                DistributionId: Guid.NewGuid(),
                IdempotencyKey: "x",
                Reason: "x",
                RequestedAt: DateTimeOffset.UnixEpoch)
        };

        await handler.HandleAsync(envelope);

        await workflowDispatcher.DidNotReceiveWithAnyArgs()
            .StartWorkflowAsync(default!, default!, default!);
    }

    [Fact]
    public async Task SameEventId_TwiceDelivered_StartsWorkflowOnce()
    {
        var workflowDispatcher = Substitute.For<IWorkflowDispatcher>();
        workflowDispatcher.StartWorkflowAsync(
                Arg.Any<string>(),
                Arg.Any<object>(),
                Arg.Any<DomainRoute>())
            .Returns(Task.FromResult(WorkflowResult.Success()));

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new PayoutFailureCompensationIntegrationHandler(
            workflowDispatcher, idempotency);

        var eventId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var envelope = BuildFailedEnvelope(
            Guid.NewGuid(), Guid.NewGuid(), "retry_exhausted", eventId);

        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        await workflowDispatcher.Received(1).StartWorkflowAsync(
            Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DomainRoute>());
    }

    [Fact]
    public async Task DispatcherThrows_ReleasesClaim_NextAttemptStartsCleanly()
    {
        var workflowDispatcher = Substitute.For<IWorkflowDispatcher>();
        var calls = 0;
        workflowDispatcher.StartWorkflowAsync(
                Arg.Any<string>(), Arg.Any<object>(), Arg.Any<DomainRoute>())
            .Returns(_ =>
            {
                calls++;
                return calls == 1
                    ? Task.FromException<WorkflowResult>(new InvalidOperationException("engine hiccup"))
                    : Task.FromResult(WorkflowResult.Success());
            });

        var idempotency = new InMemoryIdempotencyStore();
        var handler = new PayoutFailureCompensationIntegrationHandler(
            workflowDispatcher, idempotency);

        var envelope = BuildFailedEnvelope(Guid.NewGuid(), Guid.NewGuid(), "network");

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(envelope));
        await handler.HandleAsync(envelope);

        Assert.Equal(2, calls);
    }
}
