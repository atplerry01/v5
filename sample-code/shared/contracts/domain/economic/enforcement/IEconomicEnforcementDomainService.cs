namespace Whycespace.Shared.Contracts.Domain.Economic.Enforcement;

public interface IEconomicEnforcementDomainService
{
    Task<DomainOperationResult> ApplyAsync(DomainExecutionContext context, string id, string identityId, string reason, string enforcementType, string scope, string duration);
    Task<DomainOperationResult> ReleaseAsync(DomainExecutionContext context, string id);
}

public sealed record EnforcementApplyData(
    string EnforcementId,
    string IdentityId,
    string EnforcementType,
    string Scope,
    string Duration,
    string Status,
    string Decision,
    string? ReasonCode);
