using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Engines.T0U.WhycePolicy.Registry;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.WhycePolicy.Evaluation;

public sealed class PolicyEvaluationHandler : IPolicyEvaluationEngine
{
    private readonly IPolicyRegistryEngine _registryEngine;
    private readonly IPolicyEvaluationDomainService _evaluationDomainService;
    private readonly IPolicyRegistryDomainService _registryDomainService;
    private readonly IClock _clock;

    public PolicyEvaluationHandler(
        IPolicyRegistryEngine registryEngine,
        IPolicyEvaluationDomainService evaluationDomainService,
        IPolicyRegistryDomainService registryDomainService,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(registryEngine);
        ArgumentNullException.ThrowIfNull(evaluationDomainService);
        ArgumentNullException.ThrowIfNull(registryDomainService);

        _registryEngine = registryEngine;
        _evaluationDomainService = evaluationDomainService;
        _registryDomainService = registryDomainService;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<PolicyEvaluationEngineResult> EvaluateAsync(
        PolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. Resolve policies via registry
        IReadOnlyList<ResolvedPolicy> candidates;
        if (context.PolicyId.HasValue)
        {
            var single = await _registryEngine.FindByIdAsync(context.PolicyId.Value, cancellationToken);
            candidates = single is not null ? [single] : [];
        }
        else
        {
            candidates = await _registryEngine.ResolvePoliciesAsync(context, cancellationToken);
        }

        // PATCH 1: Default DENY — no policies = no explicit allow = DENY
        if (candidates.Count == 0)
        {
            return new PolicyEvaluationEngineResult(
                PolicyDecisionType.Deny,
                [],
                ["No applicable policies found — default DENY"],
                BuildEventPayload(Guid.Empty, PolicyDecisionType.Deny, context));
        }

        // Build domain execution context from policy context
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{context.ActorId}:{context.Action}:{context.Resource}:EvaluatePolicy").ToString("N"),
            ActorId = context.ActorId.ToString(),
            Action = context.Action,
            Domain = "constitutional.policy",
            Timestamp = _clock.UtcNowOffset
        };

        // PATCH 4: Activation filter — only active policies with active, effective versions
        var now = context.Timestamp;
        var policies = new List<ResolvedPolicy>();
        foreach (var p in candidates)
        {
            if (p.ActiveVersionId != Guid.Empty)
            {
                var effective = await _registryDomainService.CheckVersionEffectiveAsync(execCtx, p.ActiveVersionId, now);
                if (effective)
                    policies.Add(p);
            }
            else
            {
                policies.Add(p);
            }
        }

        if (policies.Count == 0)
        {
            return new PolicyEvaluationEngineResult(
                PolicyDecisionType.Deny,
                [],
                ["No active policies with effective versions — default DENY"],
                BuildEventPayload(Guid.Empty, PolicyDecisionType.Deny, context));
        }

        // PATCH 3: Sort by priority descending (highest first)
        policies.Sort((a, b) => b.Priority.CompareTo(a.Priority));

        // Evaluate per-policy, collecting results keyed by priority
        var policyResults = new List<(ResolvedPolicy Policy, string Decision, List<EvaluatedRuleResult> Rules, List<string> Violations)>();

        foreach (var policy in policies)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 2. Filter by scope applicability
            if (policy.ScopeId != Guid.Empty)
            {
                var scopeApplies = await _registryDomainService.CheckScopeAppliesAsync(execCtx, policy.ScopeId, context.Resource, context.Action);
                if (!scopeApplies)
                    continue;
            }

            var ruleResults = new List<EvaluatedRuleResult>();
            var violations = new List<string>();
            var hasDeny = false;

            // 3. Evaluate constraints via domain service
            var facts = ConstraintEvaluator.BuildFacts(context);
            var constraintResult = await _evaluationDomainService.EvaluateConstraintAsync(execCtx, "policy_rules", facts);

            if (!constraintResult.Passed)
            {
                ruleResults.Add(new EvaluatedRuleResult(Guid.Empty, false, constraintResult.Reason));
                violations.Add(constraintResult.Reason ?? $"Constraint failed for policy {policy.PolicyId}");
                hasDeny = true;
            }
            else
            {
                ruleResults.Add(new EvaluatedRuleResult(Guid.Empty, true));
            }

            var decision = hasDeny ? PolicyDecisionType.Deny : PolicyDecisionType.Allow;
            policyResults.Add((policy, decision, ruleResults, violations));
        }

        // PATCH 3: Conflict resolution — highest priority wins; equal priority: DENY > CONDITIONAL > ALLOW
        var finalResult = ResolveConflicts(policyResults, context);
        return finalResult;
    }

    private PolicyEvaluationEngineResult ResolveConflicts(
        List<(ResolvedPolicy Policy, string Decision, List<EvaluatedRuleResult> Rules, List<string> Violations)> policyResults,
        PolicyContext context)
    {
        if (policyResults.Count == 0)
        {
            return new PolicyEvaluationEngineResult(
                PolicyDecisionType.Deny,
                [],
                ["No scope-applicable policies — default DENY"],
                BuildEventPayload(Guid.Empty, PolicyDecisionType.Deny, context));
        }

        // Already sorted by priority descending — take highest priority group
        var highestPriority = policyResults[0].Policy.Priority;
        var topGroup = policyResults
            .Where(r => r.Policy.Priority == highestPriority)
            .ToList();

        // Within equal priority: DENY wins over CONDITIONAL wins over ALLOW
        string finalDecision;
        if (topGroup.Any(r => r.Decision == PolicyDecisionType.Deny))
            finalDecision = PolicyDecisionType.Deny;
        else if (topGroup.Any(r => r.Decision == PolicyDecisionType.Conditional))
            finalDecision = PolicyDecisionType.Conditional;
        else
            finalDecision = PolicyDecisionType.Allow;

        var allRules = topGroup.SelectMany(r => r.Rules).ToList();
        var allViolations = topGroup.SelectMany(r => r.Violations).ToList();
        var winningPolicyId = topGroup[0].Policy.PolicyId;

        // PATCH 5: Build event payload — engine does NOT publish, runtime does
        var eventPayload = BuildEventPayload(winningPolicyId, finalDecision, context);

        return new PolicyEvaluationEngineResult(finalDecision, allRules, allViolations, eventPayload);
    }

    private static PolicyEvaluatedEventPayload BuildEventPayload(
        Guid policyId, string decision, PolicyContext context)
    {
        return new PolicyEvaluatedEventPayload(
            policyId,
            decision,
            context.ActorId,
            context.Action,
            context.Resource,
            context.Environment,
            context.Timestamp);
    }
}
