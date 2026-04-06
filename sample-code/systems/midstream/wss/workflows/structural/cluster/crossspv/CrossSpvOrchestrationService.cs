using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Contracts.Workflow;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// E18.6.3 — Systems-layer orchestration service for cross-SPV workflows.
/// Composition only: dispatches workflow intents to the runtime via IWorkflowRouter.
/// No execution, no domain mutation.
/// </summary>
public sealed class CrossSpvOrchestrationService
{
    private readonly IWorkflowRouter _router;
    private readonly IClock _clock;

    public CrossSpvOrchestrationService(IWorkflowRouter router, IClock clock)
    {
        _router = router;
        _clock = clock;
    }

    public async Task<IntentResult> StartCrossSpvWorkflowAsync(
        Guid transactionId,
        Guid rootSpvId,
        string correlationId,
        string cluster,
        string subcluster,
        string? whyceId = null,
        string? policyId = null,
        CancellationToken ct = default)
    {
        var workflowContext = new CrossSpvWorkflowContext
        {
            TransactionId = transactionId,
            RootSpvId = rootSpvId,
            CorrelationId = correlationId,
            StepId = "prepare",
            State = "pending",
            Transition = "start"
        };

        var request = new WorkflowDispatchRequest
        {
            WorkflowId = CrossSpvWorkflowDefinition.WorkflowId,
            CommandType = "CrossSpvExecution",
            Payload = workflowContext,
            CorrelationId = correlationId,
            Cluster = cluster,
            Subcluster = subcluster,
            Domain = "structural",
            Context = "cluster.crossspv",
            Timestamp = _clock.UtcNowOffset,
            AggregateId = transactionId.ToString(),
            WhyceId = whyceId,
            PolicyId = policyId
        };

        return await _router.RouteAsync(request, ct);
    }
}
