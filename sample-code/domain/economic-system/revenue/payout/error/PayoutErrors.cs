namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutException : DomainException
{
    public PayoutException(string message) : base("PAYOUT_ERROR", message) { }
}

public static class PayoutErrors
{
    public static PayoutException InsufficientFundsForPayout() =>
        new("Insufficient funds to process the payout.");

    public static PayoutException PayoutNotApproved() =>
        new("Payout must be approved before processing.");

    public static PayoutException PayoutAlreadyProcessed() =>
        new("Payout has already been processed and cannot be modified.");
}
