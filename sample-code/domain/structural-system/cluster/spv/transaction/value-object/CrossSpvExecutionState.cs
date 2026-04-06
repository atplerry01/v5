namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record CrossSpvExecutionState(string Value)
{
    public static readonly CrossSpvExecutionState Pending = new("pending");
    public static readonly CrossSpvExecutionState Preparing = new("preparing");
    public static readonly CrossSpvExecutionState Executing = new("executing");
    public static readonly CrossSpvExecutionState Committing = new("committing");
    public static readonly CrossSpvExecutionState Compensating = new("compensating");
    public static readonly CrossSpvExecutionState Completed = new("completed");
    public static readonly CrossSpvExecutionState Failed = new("failed");

    public bool IsTerminal => this == Completed || this == Failed;
}
