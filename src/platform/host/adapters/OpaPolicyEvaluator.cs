using System.Text.Json;
using Whyce.Shared.Contracts.Infrastructure.Policy;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// OPA (Open Policy Agent) backed policy evaluator.
/// Sends policy evaluation requests to the OPA REST API.
/// </summary>
public sealed class OpaPolicyEvaluator : IPolicyEvaluator
{
    private readonly HttpClient _httpClient;
    private readonly string _opaEndpoint;

    public OpaPolicyEvaluator(HttpClient httpClient, string opaEndpoint)
    {
        _httpClient = httpClient;
        _opaEndpoint = opaEndpoint.TrimEnd('/');
    }

    public async Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        // Map CommandType to OPA action (e.g., "CreateTodoCommand" → "todo.create")
        var action = MapCommandTypeToAction(policyContext.CommandType, policyContext.Domain);

        var requestBody = new
        {
            input = new
            {
                policy_id = policyId,
                action,
                subject = new
                {
                    role = policyContext.Roles.FirstOrDefault() ?? "anonymous"
                },
                resource = new
                {
                    classification = policyContext.Classification,
                    context = policyContext.Context,
                    domain = policyContext.Domain
                },
                correlation_id = policyContext.CorrelationId.ToString(),
                tenant_id = policyContext.TenantId,
                actor_id = policyContext.ActorId
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // OPA policy path matches domain route: /v1/data/whyce/policy/{classification}/{context}/{domain}
        var policyPath = policyId.Replace('.', '/');
        var response = await _httpClient.PostAsync($"{_opaEndpoint}/v1/data/whyce/policy/{policyPath}", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var opaResult = JsonSerializer.Deserialize<OpaResponse>(responseBody);

        var isAllowed = opaResult?.Result?.Allow ?? false;
        var decisionHash = ComputeDecisionHash(policyId, policyContext, isAllowed);
        var denialReason = isAllowed ? null : (opaResult?.Result?.DenialReason ?? "Policy denied by OPA");

        return new PolicyDecision(isAllowed, policyId, decisionHash, denialReason);
    }

    private static string ComputeDecisionHash(string policyId, PolicyContext context, bool allowed)
    {
        var seed = $"{policyId}:{context.CorrelationId}:{context.CommandType}:{allowed}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }

    private static string MapCommandTypeToAction(string commandType, string domain)
    {
        // Strip "Command" suffix and extract verb: "CreateTodoCommand" → "create"
        var name = commandType.Replace("Command", "", StringComparison.Ordinal);

        // Find the verb by removing the domain name portion
        // e.g., "CreateTodo" with domain "todo" → verb = "create"
        var domainIndex = name.IndexOf(domain, StringComparison.OrdinalIgnoreCase);
        var verb = domainIndex > 0
            ? name[..domainIndex].ToLowerInvariant()
            : name.ToLowerInvariant();

        // Also handle WorkflowStartCommand which wraps the actual command
        if (commandType == "WorkflowStartCommand")
            verb = "create";

        return $"{domain}.{verb}";
    }

    private sealed class OpaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("result")]
        public OpaResultPayload? Result { get; set; }
    }

    private sealed class OpaResultPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("allow")]
        public bool Allow { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("deny")]
        public bool Deny { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("denial_reason")]
        public string? DenialReason { get; set; }
    }
}
