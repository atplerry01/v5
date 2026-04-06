using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Trust;

public interface IFederationDomainService
{
    bool IsIssuerActive(DomainExecutionContext context, string issuerStatus);
    bool IsTrustStatusValid(DomainExecutionContext context, string trustStatus);
    bool IsProvenanceValid(DomainExecutionContext context, string provenanceSource);
}
