namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// Canonical string tokens for the kinds of economic actions a
/// transaction can orchestrate. The aggregate stores `Kind` as a string
/// to stay open to future action types without a breaking enum change;
/// callers SHOULD use these constants.
/// </summary>
public static class TransactionKind
{
    public const string Expense = "expense";
    public const string Revenue = "revenue";
    public const string Distribution = "distribution";
    public const string Payout = "payout";
    public const string Charge = "charge";
}
