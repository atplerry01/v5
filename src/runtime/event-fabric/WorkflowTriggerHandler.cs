using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Event-to-workflow trigger handler. Inspects the EventType field of each
/// incoming envelope and, for recognized economic events, starts the
/// corresponding T1M workflow via <see cref="IWorkflowDispatcher"/>. The
/// handler does NOT execute domain logic or mutate aggregates; it is a
/// pure routing layer.
///
/// Wired-in state: class exists and compiles. NOT YET attached to a Kafka
/// consumer worker — the next pass will extend the runtime consumer
/// pipeline to subscribe to whyce.economic.*.events topics and dispatch
/// matching envelopes through this handler. See Phase 2E follow-ups.
/// </summary>
public sealed class WorkflowTriggerHandler
{
    private static readonly DomainRoute RevenueRoute = new("economic", "revenue", "revenue");
    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    private readonly IWorkflowDispatcher _workflowDispatcher;

    public WorkflowTriggerHandler(IWorkflowDispatcher workflowDispatcher)
    {
        _workflowDispatcher = workflowDispatcher;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        switch (envelope.EventType)
        {
            case "RevenueRecordedEvent":
                // Trigger only — the workflow itself re-validates.
                // The envelope's payload is the source-of-truth for fields; the
                // workflow step validates and hydrates typed state.
                await _workflowDispatcher.StartWorkflowAsync(
                    RevenueProcessingWorkflowNames.Process,
                    envelope.Payload,
                    RevenueRoute);
                break;

            case "PayoutExecutedEvent":
                await _workflowDispatcher.StartWorkflowAsync(
                    PayoutExecutionWorkflowNames.Execute,
                    envelope.Payload,
                    PayoutRoute);
                break;

            case "DistributionCreatedEvent":
                // Distribution is creation-triggered, not event-triggered.
                // Left as an explicit no-op per prompt ("optional trigger (manual/API)").
                break;
        }
    }
}
