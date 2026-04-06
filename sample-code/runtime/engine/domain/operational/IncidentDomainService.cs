using Whycespace.Shared.Primitives.Time;
using Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Operational.Incident;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.Engine.Domain.Operational;

public sealed class IncidentDomainService : GovernedDomainServiceBase, IIncidentDomainService
{
    private readonly IAggregateStore _aggregateStore;

    public IncidentDomainService(
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

    public async Task<DomainOperationResult> CreateAsync(
        DomainExecutionContext context,
        string id,
        string title,
        string description,
        string type,
        string severity,
        string source,
        string? referenceId,
        string? correlationId)
    {
        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var incidentId = Guid.Parse(id);
            var incident = IncidentBaseAggregate.Create(
                incidentId,
                new IncidentType(type),
                new IncidentSeverity(severity),
                new IncidentSource(source),
                incidentId,
                description,
                referenceId is not null
                    ? new IncidentReference(referenceId, Guid.Empty)
                    : IncidentReference.None,
                correlationId is not null
                    ? new IncidentCorrelationId(correlationId)
                    : default);

            await _aggregateStore.SaveAsync(incident);

            return (incidentId, (object?)new
            {
                AggregateId = id,
                IncidentType = type,
                Severity = severity,
                Priority = IncidentPriority.FromSeverity(new IncidentSeverity(severity)).Value,
                Source = source,
                Status = "created"
            });
        });
    }
}
