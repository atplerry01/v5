namespace Whycespace.Shared.Contracts.Economic.Transaction.Workflow;

public static class TransactionLifecycleWorkflowNames
{
    public const string Lifecycle = "economic.transaction.lifecycle";
}

public static class TransactionLifecycleSteps
{
    public const string ExecuteInstruction  = "execute_instruction";
    public const string InitiateTransaction = "initiate_transaction";
    public const string CheckLimit          = "check_limit";
    public const string InitiateSettlement  = "initiate_settlement";
    public const string FxLock              = "fx_lock";
    public const string PostToLedger        = "post_to_ledger";
}
