using System.Diagnostics;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Policy evaluation and enforcement distributed tracing.
/// No business logic — observability only.
/// </summary>
public sealed class PolicyTracing
{
    private static readonly ActivitySource Source = new("Whycespace.Policy");

    public static Activity? BeginEvaluation(string action, string classification)
    {
        return Source.StartActivity(
            $"policy.evaluate.{classification}",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("policy.action", action),
                new("policy.classification", classification)
            ]);
    }

    public static void EndEvaluation(Activity? activity, string decisionType, int ruleCount, int violationCount)
    {
        if (activity is null) return;

        activity.SetTag("policy.decision", decisionType);
        activity.SetTag("policy.rules.count", ruleCount);
        activity.SetTag("policy.violations.count", violationCount);
        activity.SetStatus(decisionType == "DENY" ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
        activity.Stop();
    }

    public static Activity? BeginEnforcement(string actionType, string targetType, string severity)
    {
        return Source.StartActivity(
            $"policy.enforce.{actionType}",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("enforcement.type", actionType),
                new("enforcement.target", targetType),
                new("enforcement.severity", severity)
            ]);
    }

    public static void EndEnforcement(Activity? activity, bool dispatched)
    {
        if (activity is null) return;

        activity.SetTag("enforcement.dispatched", dispatched);
        activity.SetStatus(dispatched ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity.Stop();
    }

    public static Activity? BeginActivation(Guid policyId, int version)
    {
        return Source.StartActivity(
            "policy.activate",
            ActivityKind.Internal,
            parentContext: default,
            tags:
            [
                new("policy.id", policyId.ToString()),
                new("policy.version", version)
            ]);
    }

    public static void EndActivation(Activity? activity, bool success, string? reason = null)
    {
        if (activity is null) return;

        activity.SetTag("activation.success", success);
        if (reason is not null)
            activity.SetTag("activation.failure_reason", reason);
        activity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity.Stop();
    }

    public static Activity? BeginRegistryLoad()
    {
        return Source.StartActivity(
            "policy.registry.load",
            ActivityKind.Internal);
    }

    public static void EndRegistryLoad(Activity? activity, int policyCount, bool fromSnapshot)
    {
        if (activity is null) return;

        activity.SetTag("registry.policy_count", policyCount);
        activity.SetTag("registry.from_snapshot", fromSnapshot);
        activity.SetStatus(ActivityStatusCode.Ok);
        activity.Stop();
    }
}
