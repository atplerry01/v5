namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public class EconomicLimitAggregate : AggregateRoot
{
    private IdentityId _identityId = null!;
    private MaxTransactionAmount _maxTransaction = null!;
    private DailyLimit _dailyLimit = null!;
    private MonthlyLimit _monthlyLimit = null!;
    private LimitSource _source;

    public void Create(Guid id, IdentityId identityId, MaxTransactionAmount maxTransaction, DailyLimit dailyLimit, MonthlyLimit monthlyLimit, LimitSource source = LimitSource.Policy)
    {
        EnsureInvariant(id != Guid.Empty, "LimitId", "LimitId cannot be empty.");
        ArgumentNullException.ThrowIfNull(identityId);
        ArgumentNullException.ThrowIfNull(maxTransaction);
        ArgumentNullException.ThrowIfNull(dailyLimit);
        ArgumentNullException.ThrowIfNull(monthlyLimit);

        Id = id;
        _identityId = identityId;
        _maxTransaction = maxTransaction;
        _dailyLimit = dailyLimit;
        _monthlyLimit = monthlyLimit;
        _source = source;

        RaiseDomainEvent(new LimitAssignedEvent(
            id, identityId.Value,
            maxTransaction.Value.Value,
            dailyLimit.Value.Value,
            monthlyLimit.Value.Value));
    }

    public void CheckTransaction(Amount transactionAmount)
    {
        if (_maxTransaction.IsExceededBy(transactionAmount))
        {
            RaiseDomainEvent(new LimitExceededEvent(
                Id, _identityId.Value, "MaxTransaction",
                transactionAmount.Value, _maxTransaction.Value.Value));
        }
    }

    public void CheckDailyUsage(Amount dailyTotal)
    {
        if (_dailyLimit.IsExceededBy(dailyTotal))
        {
            RaiseDomainEvent(new LimitExceededEvent(
                Id, _identityId.Value, "Daily",
                dailyTotal.Value, _dailyLimit.Value.Value));
        }
    }

    public void CheckMonthlyUsage(Amount monthlyTotal)
    {
        if (_monthlyLimit.IsExceededBy(monthlyTotal))
        {
            RaiseDomainEvent(new LimitExceededEvent(
                Id, _identityId.Value, "Monthly",
                monthlyTotal.Value, _monthlyLimit.Value.Value));
        }
    }
}
