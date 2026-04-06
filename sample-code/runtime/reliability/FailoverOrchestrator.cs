namespace Whycespace.Runtime.Reliability;

/// <summary>
/// Orchestrates failover procedures when continuity is triggered.
/// Routes commands to the continuity domain aggregate via the standard runtime pipeline.
/// Does NOT contain business logic — delegates to domain.
/// </summary>
public sealed class FailoverOrchestrator
{
    /// <summary>
    /// Produces the sequence of failover steps to execute.
    /// Each step is a command that flows through the runtime pipeline.
    /// </summary>
    public FailoverPlan CreateFailoverPlan(ContinuityTriggerCommand trigger, FailoverContext context)
    {
        ArgumentNullException.ThrowIfNull(trigger);
        ArgumentNullException.ThrowIfNull(context);

        var steps = new List<FailoverStep>();

        // Step 1: Activate continuity plan
        steps.Add(new FailoverStep(
            Order: 1,
            Action: "ACTIVATE_CONTINUITY_PLAN",
            TargetId: context.PlanId,
            Description: $"Activate continuity plan for cluster {trigger.ClusterId}"));

        // Step 2: Trigger failover to primary target
        if (context.PrimaryTargetId != Guid.Empty)
        {
            steps.Add(new FailoverStep(
                Order: 2,
                Action: "TRIGGER_FAILOVER",
                TargetId: context.PrimaryTargetId,
                Description: $"Failover to primary target: {trigger.Reason}"));
        }

        // Step 3: Start recovery procedure
        steps.Add(new FailoverStep(
            Order: 3,
            Action: "START_RECOVERY",
            TargetId: context.PlanId,
            Description: "Initiate recovery procedure"));

        // Step 4: Notify governance
        steps.Add(new FailoverStep(
            Order: 4,
            Action: "NOTIFY_GOVERNANCE",
            TargetId: trigger.ClusterId,
            Description: $"Notify governance of {trigger.TriggerType} failover"));

        return new FailoverPlan(
            ClusterId: trigger.ClusterId,
            TriggerReason: trigger.Reason,
            Severity: trigger.Severity,
            Steps: steps);
    }
}

public sealed record FailoverContext(
    Guid PlanId,
    Guid PrimaryTargetId,
    Guid SecondaryTargetId);

public sealed record FailoverPlan(
    Guid ClusterId,
    string TriggerReason,
    string Severity,
    IReadOnlyList<FailoverStep> Steps);

public sealed record FailoverStep(
    int Order,
    string Action,
    Guid TargetId,
    string Description);
