using Whycespace.Domain.ConstitutionalSystem.Policy.Rule;
using Whycespace.Domain.ConstitutionalSystem.Policy.Version;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class PolicyRegistryService
{
    private readonly IReadOnlyList<PolicyAggregate> _policies;
    private readonly IReadOnlyList<PolicyVersionAggregate> _versions;

    // Indexed lookups for deterministic resolution
    private readonly Dictionary<Guid, PolicyAggregate> _byId;
    private readonly ILookup<Guid, PolicyAggregate> _byScopeId;
    private readonly ILookup<string, PolicyAggregate> _byClassification;

    public PolicyRegistryService(
        IReadOnlyList<PolicyAggregate> policies,
        IReadOnlyList<PolicyVersionAggregate>? versions = null)
    {
        ArgumentNullException.ThrowIfNull(policies);
        _policies = policies;
        _versions = versions ?? [];

        _byId = policies.ToDictionary(p => p.Id);
        _byScopeId = policies.ToLookup(p => p.ScopeId);
        _byClassification = policies.ToLookup(p => ExtractClassification(p.Name));
    }

    public PolicyAggregate? FindById(Guid policyId)
    {
        return _byId.GetValueOrDefault(policyId);
    }

    public IReadOnlyList<PolicyAggregate> FindByScope(Guid scopeId)
    {
        return _byScopeId[scopeId]
            .Where(p => p.Status == PolicyStatus.Active)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<PolicyAggregate> FindByClassification(string classification)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(classification);

        return _byClassification[classification.ToLowerInvariant()]
            .Where(p => p.Status == PolicyStatus.Active)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<PolicyAggregate> ResolveActivePolicies()
    {
        return _policies
            .Where(p => p.Status == PolicyStatus.Active)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<PolicyAggregate> ResolveApplicable(Guid scopeId)
    {
        return _byScopeId[scopeId]
            .Where(p => p.Status == PolicyStatus.Active)
            .OrderByDescending(p => p.Priority.Value)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<PolicyAggregate> ResolveEffective(DateTimeOffset pointInTime)
    {
        return _policies
            .Where(p => p.Status == PolicyStatus.Active)
            .Where(p => HasEffectiveVersion(p, pointInTime))
            .OrderByDescending(p => p.Priority.Value)
            .ToList()
            .AsReadOnly();
    }

    public PolicyVersionAggregate? ResolveActiveVersion(Guid policyId)
    {
        var policy = FindById(policyId);
        if (policy is null || policy.ActiveVersionId == Guid.Empty)
            return null;

        return _versions.FirstOrDefault(v =>
            v.Id == policy.ActiveVersionId &&
            v.Status == VersionStatus.Active);
    }

    private bool HasEffectiveVersion(PolicyAggregate policy, DateTimeOffset pointInTime)
    {
        if (policy.ActiveVersionId == Guid.Empty)
            return true;

        var version = _versions.FirstOrDefault(v => v.Id == policy.ActiveVersionId);
        return version is null || version.IsEffectiveAt(pointInTime);
    }

    private static string ExtractClassification(string policyName)
    {
        var dotIndex = policyName.IndexOf('.', StringComparison.Ordinal);
        return dotIndex > 0
            ? policyName[..dotIndex].ToLowerInvariant()
            : policyName.ToLowerInvariant();
    }
}
