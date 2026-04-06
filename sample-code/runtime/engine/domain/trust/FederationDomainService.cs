using Whycespace.Domain.TrustSystem.Identity.Federation;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Trust;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Engine.Domain.Trust;

/// <summary>
/// Runtime implementation of IFederationDomainService.
/// Pure validation methods — no aggregate mutation, no chain anchoring.
/// Uses NoPolicyFlag and emits observability signals for audit compliance.
/// </summary>
public sealed class FederationDomainService : IFederationDomainService
{
    private readonly EnforcementMetrics _metrics;
    private readonly EnforcementAnomalyEmitter _anomalyEmitter;
    private readonly IClock _clock;

    public FederationDomainService(EnforcementMetrics metrics, EnforcementAnomalyEmitter anomalyEmitter, IClock clock)
    {
        _metrics = metrics;
        _anomalyEmitter = anomalyEmitter;
        _clock = clock;
    }

    public bool IsIssuerActive(DomainExecutionContext context, string issuerStatus)
    {
        ValidateWithNoPolicyGuard(context, "IsIssuerActive");
        var result = issuerStatus == IssuerStatus.Approved.ToString();
        _metrics.RecordEnforcementOutcome(result ? "federation_pass" : "federation_fail", "IsIssuerActive", 0);
        return result;
    }

    public bool IsTrustStatusValid(DomainExecutionContext context, string trustStatus)
    {
        ValidateWithNoPolicyGuard(context, "IsTrustStatusValid");
        var result = trustStatus != FederationTrustStatus.Suspended.ToString();
        _metrics.RecordEnforcementOutcome(result ? "federation_pass" : "federation_fail", "IsTrustStatusValid", 0);
        return result;
    }

    public bool IsProvenanceValid(DomainExecutionContext context, string provenanceSource)
    {
        ValidateWithNoPolicyGuard(context, "IsProvenanceValid");
        var result = provenanceSource != ProvenanceSource.SystemInferred.ToString();
        _metrics.RecordEnforcementOutcome(result ? "federation_pass" : "federation_fail", "IsProvenanceValid", 0);
        return result;
    }

    private void ValidateWithNoPolicyGuard(DomainExecutionContext context, string action)
    {
        context.Validate();

        if (!context.NoPolicyFlag)
        {
            _anomalyEmitter.Emit(new EnforcementAnomalySignal
            {
                Type = "FEDERATION_MISSING_NO_POLICY_FLAG",
                CorrelationId = context.CorrelationId,
                Description = $"Federation validation '{action}' called without NoPolicyFlag — potential governance bypass",
                CommandType = action,
                Timestamp = _clock.UtcNowOffset
            });
        }
    }
}
