namespace Whycespace.Shared.Contracts.Economic.Transaction.Expense.Workflow;

public sealed record ExpenseRecordingIntent(
    Guid ExpenseId,
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference);

public static class ExpenseRecordingWorkflowNames
{
    public const string Record = "economic.transaction.expense.record";
}
