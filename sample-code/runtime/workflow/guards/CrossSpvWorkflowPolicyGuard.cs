using Whycespace.Runtime.Workflow.Cluster;
using Whycespace.Shared.Contracts.Workflow;

namespace Whycespace.Runtime.Workflow.Guards;

/// <summary>
/// E18.6.4 — Workflow policy guard for cross-SPV execution.
/// Validates workflow context before policy evaluation.
/// READ-ONLY: does not invoke policy — that happens in PolicyMiddleware.
/// </summary>
public sealed class CrossSpvWorkflowPolicyGuard
{
    public static void Guard(IWorkflowContext context)
    {
        if (context is not CrossSpvWorkflowContext c)
            throw new InvalidOperationException("Invalid workflow context: expected CrossSpvWorkflowContext");

        if (c.TransactionId == Guid.Empty)
            throw new InvalidOperationException("TransactionId required for cross-SPV workflow");

        if (c.RootSpvId == Guid.Empty)
            throw new InvalidOperationException("RootSpvId required for cross-SPV workflow");

        if (string.IsNullOrWhiteSpace(c.CorrelationId))
            throw new InvalidOperationException("CorrelationId required for cross-SPV workflow");
    }
}
