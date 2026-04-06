using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Platform.Api.Core.Services;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Platform.Api.Core.Mappers;

/// <summary>
/// Maps WhyceRequest + IntentRoute + identity + preview into WorkflowStartRequest.
/// Pure mapping — no business logic, no side effects.
/// WorkflowId is deterministic: derived from correlationId + workflowKey + identityId.
/// </summary>
public static class IntentMapper
{
    public static WorkflowStartRequest ToWorkflowStartRequest(
        WhyceRequest request,
        IntentRoute route,
        string correlationId,
        string traceId,
        string whyceId,
        WhyceIdentity? identity = null,
        PolicyPreview? preview = null,
        TenantContext? tenant = null,
        RegionContext? region = null)
    {
        var identityId = identity?.IdentityId ?? DeterministicIdHelper.FromSeed($"identity:{whyceId}");

        // Deterministic WorkflowId: same inputs always produce the same ID
        var workflowId = DeterministicIdHelper.FromSeed(
            $"workflow:{correlationId}:{route.WorkflowKey}:{identityId}");

        // Build metadata from preview + intent data + tenant/region
        Dictionary<string, string>? metadata = null;
        if (preview is not null || request.IntentData is not null || tenant is not null || region is not null)
        {
            metadata = new Dictionary<string, string>();

            if (preview is not null)
            {
                metadata["policyDecision"] = preview.Decision;
                if (preview.PolicyId is not null)
                    metadata["policyId"] = preview.PolicyId;
            }

            if (request.IntentData is not null)
            {
                foreach (var (key, value) in request.IntentData)
                    metadata[$"intent.{key}"] = value;
            }

            if (tenant is not null)
            {
                metadata["tenantId"] = tenant.TenantId.ToString();
                metadata["tenantType"] = tenant.TenantType;
            }

            if (region is not null)
            {
                metadata["region"] = region.Region;
                metadata["jurisdiction"] = region.Jurisdiction;
            }
        }

        return new WorkflowStartRequest
        {
            WorkflowId = workflowId,
            WorkflowKey = route.WorkflowKey,
            CommandType = route.CommandType,
            Payload = request.Payload,
            Cluster = route.Cluster,
            Authority = route.Authority,
            SubCluster = route.SubCluster,
            Domain = route.Domain,
            ExecutionTarget = route.ExecutionTarget,
            IdentityId = identityId,
            WhyceId = whyceId,
            CorrelationId = correlationId,
            TraceId = traceId,
            Jurisdiction = request.Jurisdiction,
            Tenant = tenant,
            Region = region,
            Roles = identity?.Roles,
            Attributes = identity?.Attributes,
            TrustScore = identity?.TrustScore,
            Consents = identity?.Consents,
            Metadata = metadata
        };
    }
}
