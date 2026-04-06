namespace Whycespace.Shared.Contracts.Policy;

public interface IPolicyRepository
{
    Task CreatePolicyAsync(PolicyRecord record, CancellationToken cancellationToken = default);
    Task<PolicyRecord?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyRecord>> GetPoliciesByDomainAsync(string domain, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyRecord>> GetAllPoliciesAsync(CancellationToken cancellationToken = default);
}

public sealed record PolicyRecord(
    Guid Id,
    string Name,
    string Domain,
    DateTimeOffset CreatedAt);
