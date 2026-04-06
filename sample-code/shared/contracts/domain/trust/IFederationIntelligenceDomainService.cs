using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Trust;

public interface IFederationIntelligenceDomainService
{
    decimal NormalizeTrustScore(DomainExecutionContext context, decimal rawScore, string issuerType, decimal reputationScore, string trajectoryTrend, decimal volatility);
    decimal ComputeIssuerReputation(DomainExecutionContext context, decimal credentialValidityRate, decimal revocationRate, decimal incidentRate, string trajectoryTrend, decimal volatility);
}
