using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.OrchestrationSystem.Workflow.Definition;
using Whycespace.Domain.OrchestrationSystem.Workflow.Instance;
using Whycespace.Domain.OrchestrationSystem.Workflow.Step;
using Whycespace.Domain.OrchestrationSystem.Workflow.Transition;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Workflow;

public sealed class WorkflowDomainService : GovernedDomainServiceBase, IWorkflowDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public WorkflowDomainService(
        IAggregateStore aggregateStore,
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task<DomainOperationResult> CreateDefinitionAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WorkflowDefinitionAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateInstanceAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WorkflowInstanceAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateStepAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WorkflowStepAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }

    public async Task<DomainOperationResult> CreateTransitionAsync(DomainExecutionContext context, string id)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<WorkflowTransitionAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)null, (object?)null);
        });
    }
}
