using System.Text.Json;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Opa;

public static class OpaResponseParser
{
    public static PolicyEvaluationResult Parse(string responseBody, PolicyEvaluationInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(responseBody);

        var doc = JsonDocument.Parse(responseBody);

        var decision = "DENY";
        var evaluatedRules = new List<EvaluatedRule>();
        var violations = new List<string>();
        string? evaluationTrace = null;

        if (doc.RootElement.TryGetProperty("result", out var result))
        {
            if (result.ValueKind == JsonValueKind.String)
            {
                decision = result.GetString()?.ToUpperInvariant() ?? "DENY";
            }
            else if (result.ValueKind == JsonValueKind.Object)
            {
                // Decision
                if (result.TryGetProperty("decision", out var decProp))
                    decision = decProp.GetString()?.ToUpperInvariant() ?? "DENY";

                // Rules array
                if (result.TryGetProperty("rules", out var rulesProp) &&
                    rulesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in rulesProp.EnumerateArray())
                    {
                        var ruleName = r.TryGetProperty("rule", out var rn) ? rn.GetString() ?? "" : "";
                        var ruleResult = r.TryGetProperty("result", out var rr) ? rr.GetString() ?? "" : "";
                        var passed = ruleResult != "failed" && ruleResult != "denied";

                        evaluatedRules.Add(new EvaluatedRule(
                            DeterministicGuid(ruleName),
                            passed,
                            passed ? null : ruleResult));
                    }
                }

                // Violations array
                if (result.TryGetProperty("violations", out var violProp) &&
                    violProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in violProp.EnumerateArray())
                    {
                        if (v.ValueKind == JsonValueKind.String)
                        {
                            violations.Add(v.GetString() ?? "unknown violation");
                        }
                        else if (v.ValueKind == JsonValueKind.Object)
                        {
                            var rule = v.TryGetProperty("rule", out var rn) ? rn.GetString() ?? "" : "";
                            var reason = v.TryGetProperty("reason", out var rs) ? rs.GetString() ?? "" : "";
                            violations.Add($"{rule}: {reason}");
                        }
                    }
                }

                // Trace object
                if (result.TryGetProperty("trace", out var traceProp))
                {
                    evaluationTrace = traceProp.GetRawText();
                }
            }
        }

        var eventPayload = new PolicyEventData(
            input.PolicyId ?? Guid.Empty,
            decision,
            input.ActorId,
            input.Action,
            input.Resource,
            input.Environment,
            input.Timestamp);

        return decision switch
        {
            "ALLOW" => new PolicyEvaluationResult
            {
                DecisionType = "ALLOW",
                IsCompliant = true,
                EvaluatedRules = evaluatedRules,
                EventPayload = eventPayload,
                EvaluationTrace = evaluationTrace,
                Source = PolicyExecutionSource.Opa
            },
            "CONDITIONAL" => new PolicyEvaluationResult
            {
                DecisionType = "CONDITIONAL",
                IsCompliant = true,
                Violations = violations,
                EvaluatedRules = evaluatedRules,
                EventPayload = eventPayload,
                EvaluationTrace = evaluationTrace,
                Source = PolicyExecutionSource.Opa
            },
            _ => new PolicyEvaluationResult
            {
                DecisionType = "DENY",
                IsCompliant = false,
                Violation = violations.Count > 0 ? string.Join("; ", violations) : "Policy denied by OPA",
                EvaluatedRules = evaluatedRules,
                Violations = violations.Count > 0 ? violations : ["Policy denied by OPA"],
                EventPayload = eventPayload,
                EvaluationTrace = evaluationTrace,
                Source = PolicyExecutionSource.Opa
            }
        };
    }

    private static Guid DeterministicGuid(string input)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(bytes);
    }
}
