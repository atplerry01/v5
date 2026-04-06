using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhycePolicy.Registry;

/// <summary>
/// Stateless policy registry engine.
/// All queries go through IPolicyReadModel (projection-backed).
/// No cache, no locks, no mutable state.
/// Uses IPolicyRegistryDomainService for domain operations.
/// </summary>
public sealed class PolicyRegistryHandler : IPolicyRegistryEngine, IPolicyRegistryHandler
{
    private readonly IPolicyReadModel _readModel;
    private readonly IPolicyRegistryDomainService _domainService;
    private readonly IClock _clock;

    public PolicyRegistryHandler(
        IPolicyReadModel readModel,
        IPolicyRegistryDomainService domainService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(readModel);
        ArgumentNullException.ThrowIfNull(domainService);
        _readModel = readModel;
        _domainService = domainService;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<IReadOnlyList<ResolvedPolicy>> ResolvePoliciesAsync(PolicyContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{context.ActorId}:{context.Action}:ResolvePolicies").ToString("N"),
            ActorId = context.ActorId.ToString(),
            Action = context.Action,
            Domain = "constitutional.policy",
            Timestamp = _clock.UtcNowOffset
        };

        var policies = await _readModel.GetAllPoliciesAsync();
        var applicable = new List<ResolvedPolicy>();

        foreach (var record in policies)
        {
            var policy = ReconstitutePolicyFromRecord(record);
            if (policy is null) continue;

            var scopeApplies = await _domainService.CheckScopeAppliesAsync(execCtx, policy.ScopeId, context.Resource, context.Action);
            if (scopeApplies || policy.ScopeId == Guid.Empty)
            {
                var versionRecord = await _readModel.GetActiveVersionAsync(policy.PolicyId);
                var version = versionRecord is not null ? ReconstitutePolicyVersionFromRecord(versionRecord) : null;
                var resolved = new ResolvedPolicy(
                    policy.PolicyId, policy.Name, policy.Domain, policy.Priority,
                    policy.ScopeId, version?.PolicyId ?? Guid.Empty,
                    version?.VersionStatus, version?.IsLocked ?? false);
                applicable.Add(resolved);
            }
        }

        applicable.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return applicable.AsReadOnly();
    }

    public async Task<IReadOnlyList<ResolvedPolicy>> ResolveByScopeAsync(Guid scopeId, CancellationToken cancellationToken = default)
    {
        var policies = await _readModel.GetAllPoliciesAsync();
        var applicable = new List<ResolvedPolicy>();

        foreach (var record in policies)
        {
            var policy = ReconstitutePolicyFromRecord(record);
            if (policy is null || policy.ScopeId != scopeId) continue;

            var versionRecord = await _readModel.GetActiveVersionAsync(policy.PolicyId);
            var version = versionRecord is not null ? ReconstitutePolicyVersionFromRecord(versionRecord) : null;
            applicable.Add(new ResolvedPolicy(
                policy.PolicyId, policy.Name, policy.Domain, policy.Priority,
                policy.ScopeId, version?.PolicyId ?? Guid.Empty,
                version?.VersionStatus, version?.IsLocked ?? false));
        }

        return applicable
            .OrderByDescending(r => r.Priority)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyList<ResolvedPolicy>> ResolveByClassificationAsync(string classification, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(classification);

        var policies = await _readModel.GetAllPoliciesAsync();
        var applicable = new List<ResolvedPolicy>();

        foreach (var record in policies)
        {
            var policy = ReconstitutePolicyFromRecord(record);
            if (policy is null || !MatchesClassification(policy.Name, classification)) continue;

            var versionRecord = await _readModel.GetActiveVersionAsync(policy.PolicyId);
            var version = versionRecord is not null ? ReconstitutePolicyVersionFromRecord(versionRecord) : null;
            applicable.Add(new ResolvedPolicy(
                policy.PolicyId, policy.Name, policy.Domain, policy.Priority,
                policy.ScopeId, version?.PolicyId ?? Guid.Empty,
                version?.VersionStatus, version?.IsLocked ?? false));
        }

        return applicable
            .OrderByDescending(r => r.Priority)
            .ToList()
            .AsReadOnly();
    }

    public async Task<ResolvedPolicy?> FindByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var record = await _readModel.GetPolicyByIdAsync(policyId);
        if (record is null) return null;

        var policy = ReconstitutePolicyFromRecord(record);
        if (policy is null) return null;

        var versionRecord = await _readModel.GetActiveVersionAsync(policyId);
        var version = versionRecord is not null ? ReconstitutePolicyVersionFromRecord(versionRecord) : null;
        return new ResolvedPolicy(
            policy.PolicyId, policy.Name, policy.Domain, policy.Priority,
            policy.ScopeId, version?.PolicyId ?? Guid.Empty,
            version?.VersionStatus, version?.IsLocked ?? false);
    }

    // IPolicyRegistryHandler — runtime-facing contract
    private bool _initialized;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Force a read-through to warm the projection-backed read model
        await _readModel.GetAllPoliciesAsync();
        _initialized = true;
    }

    public bool HasCachedState => _initialized;

    public void InvalidateCache(Guid policyId)
    {
        // Stateless engine — invalidation is a no-op.
        // Next read will go through the projection-backed read model.
        _initialized = false;
    }

    private static bool MatchesClassification(string policyName, string classification)
    {
        var dotIndex = policyName.IndexOf('.', StringComparison.Ordinal);
        var extracted = dotIndex > 0
            ? policyName[..dotIndex]
            : policyName;

        return string.Equals(extracted, classification, StringComparison.OrdinalIgnoreCase);
    }

    private static ResolvedPolicy? ReconstitutePolicyFromRecord(PolicyRecord record)
    {
        return new ResolvedPolicy(
            record.Id, record.Name, record.Domain, 0,
            Guid.Empty, Guid.Empty, null, false);
    }

    private static ResolvedPolicy? ReconstitutePolicyVersionFromRecord(PolicyVersionRecord record)
    {
        return new ResolvedPolicy(
            record.PolicyId, "", "", 0,
            Guid.Empty, record.Id,
            record.Status, record.IsLocked);
    }
}
