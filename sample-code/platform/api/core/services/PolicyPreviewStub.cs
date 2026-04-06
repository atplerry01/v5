using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// WhycePolicy preview service implementation.
/// Delegates policy evaluation to the runtime control plane via the "policy.preview" command.
/// Maps ClassifiedIntent + IntentRoute + WhyceIdentity → PolicyInput → RuntimeControlPlane.
/// Converts the runtime result into an explainable PolicyPreview.
///
/// ADVISORY ONLY — never blocks execution.
/// If the runtime call fails, returns PolicyPreview.ServiceUnavailable().
///
/// NO policy logic. NO rule evaluation. ONLY mapping + delegation.
/// </summary>
public sealed class WhycePolicyPreviewService : IPolicyPreviewService
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public WhycePolicyPreviewService(IRuntimeControlPlane controlPlane, IClock clock)
    {
        _controlPlane = controlPlane;
        _clock = clock;
    }

    public async Task<PolicyPreview> PreviewAsync(
        ClassifiedIntent intent,
        IntentRoute route,
        WhyceIdentity? identity,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var policyInput = BuildPolicyInput(intent, route, identity, correlationId);

            var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
            {
                CommandId = DeterministicIdHelper.FromSeed($"policy-preview:{correlationId}:{intent.WorkflowKey}"),
                CommandType = "policy.preview",
                Payload = policyInput,
                CorrelationId = correlationId,
                Timestamp = _clock.UtcNowOffset,
                WhyceId = identity?.IdentityId.ToString()
            });

            if (!result.Success)
            {
                return PolicyPreview.Deny(
                    reason: result.ErrorMessage ?? "Policy evaluation returned denial",
                    violation: result.ErrorCode,
                    policyId: null);
            }

            return PolicyPreview.Allow(
                reason: "Policy preview passed",
                policyId: null,
                metadata: BuildResponseMetadata(intent, route, correlationId));
        }
        catch
        {
            // Preview failure must NEVER block execution
            return PolicyPreview.ServiceUnavailable();
        }
    }

    private object BuildPolicyInput(
        ClassifiedIntent intent,
        IntentRoute route,
        WhyceIdentity? identity,
        string correlationId)
    {
        return new
        {
            // Identity context
            IdentityId = identity?.IdentityId.ToString(),
            Roles = identity?.Roles,
            Attributes = identity?.Attributes,
            TrustScore = identity?.TrustScore,
            Consents = identity?.Consents,

            // Intent context
            Classification = intent.Classification,
            Domain = intent.Domain,
            WorkflowKey = intent.WorkflowKey,
            IntentMetadata = intent.Metadata,

            // Route context
            Cluster = route.Cluster,
            Authority = route.Authority,
            SubCluster = route.SubCluster,
            ExecutionTarget = route.ExecutionTarget,

            // Tenant + Region context (E19.14)
            TenantId = intent.Tenant?.TenantId.ToString(),
            TenantType = intent.Tenant?.TenantType,
            Region = intent.Region?.Region,
            Jurisdiction = intent.Region?.Jurisdiction,

            // Request context
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset.ToString("O")
        };
    }

    private static Dictionary<string, string> BuildResponseMetadata(
        ClassifiedIntent intent,
        IntentRoute route,
        string correlationId)
    {
        return new Dictionary<string, string>
        {
            ["correlationId"] = correlationId,
            ["workflowKey"] = intent.WorkflowKey,
            ["cluster"] = route.Cluster,
            ["authority"] = route.Authority,
            ["subCluster"] = route.SubCluster
        };
    }
}
