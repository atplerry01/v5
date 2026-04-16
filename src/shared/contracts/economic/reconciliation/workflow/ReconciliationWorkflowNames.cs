namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;

public static class ReconciliationWorkflowNames
{
    public const string Lifecycle = "economic.reconciliation.lifecycle";
}

public enum ReconciliationLifecycleState
{
    Triggered,
    Matching,
    Matched,
    Mismatched,
    Investigating,
    ResolvingDiscrepancy,
    Resolved
}
