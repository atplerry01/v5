using NSubstitute;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Runtime.EventFabric;

/// <summary>
/// Phase 6 T6.3 — pins the reconciliation closure loop. A
/// <c>DiscrepancyResolvedEvent</c> MUST cause the
/// <see cref="ReconciliationLifecycleHandler"/> to look up the owning
/// process id and dispatch a <see cref="ResolveReconciliationCommand"/>.
/// Without this dispatch, the process aggregate remains in Mismatched
/// indefinitely (orphan state).
/// </summary>
public sealed class ReconciliationLoopClosureTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public async Task HandleAsync_OnDiscrepancyResolvedEvent_DispatchesResolveReconciliationCommand()
    {
        var discrepancyId = IdGen.Generate("T6.3:discrepancy");
        var processId = IdGen.Generate("T6.3:process");

        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        dispatcher.DispatchAsync(Arg.Any<object>(), Arg.Any<DomainRoute>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CommandResult.Success(Array.Empty<object>())));

        var lookup = Substitute.For<IReconciliationWorkflowLookup>();
        lookup.FindProcessIdByDiscrepancyAsync(discrepancyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Guid?>(processId));

        var handler = new ReconciliationLifecycleHandler(dispatcher, IdGen, new TestClock(), lookup);

        var envelope = new RawTestEnvelope
        {
            EventId = IdGen.Generate("T6.3:envelope"),
            AggregateId = discrepancyId,
            CorrelationId = IdGen.Generate("T6.3:corr"),
            EventType = "DiscrepancyResolvedEvent",
        };

        await handler.HandleAsync(envelope);

        await dispatcher.Received(1).DispatchAsync(
            Arg.Is<ResolveReconciliationCommand>(c => c.ProcessId == processId),
            Arg.Is<DomainRoute>(r =>
                r.Classification == "economic"
                && r.Context == "reconciliation"
                && r.Domain == "process"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLookupReturnsNull_DoesNotDispatchAndDoesNotThrow()
    {
        var dispatcher = Substitute.For<ISystemIntentDispatcher>();
        var lookup = Substitute.For<IReconciliationWorkflowLookup>();
        lookup.FindProcessIdByDiscrepancyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Guid?>(null));

        var handler = new ReconciliationLifecycleHandler(dispatcher, IdGen, new TestClock(), lookup);

        var envelope = new RawTestEnvelope
        {
            EventId = IdGen.Generate("T6.3:orphan"),
            AggregateId = IdGen.Generate("T6.3:orphan-agg"),
            EventType = "DiscrepancyResolvedEvent",
        };

        await handler.HandleAsync(envelope);

        await dispatcher.DidNotReceiveWithAnyArgs()
            .DispatchAsync(default!, default!, Arg.Any<CancellationToken>());
    }
}
