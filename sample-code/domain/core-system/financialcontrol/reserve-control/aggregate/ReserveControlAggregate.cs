using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.ReserveControl;

public sealed class ReserveControlAggregate : AggregateRoot
{
    public string ReserveName { get; private set; } = string.Empty;
    public decimal MinimumBalance { get; private set; }

    public static ReserveControlAggregate Create(Guid id, string reserveName, decimal minimumBalance)
    {
        var agg = new ReserveControlAggregate
        {
            Id = id,
            ReserveName = reserveName,
            MinimumBalance = minimumBalance
        };
        agg.RaiseDomainEvent(new ReserveControlCreatedEvent(id, reserveName, minimumBalance));
        return agg;
    }

    public void UpdateMinimum(decimal amount)
    {
        MinimumBalance = amount;
        RaiseDomainEvent(new ReserveControlMinimumUpdatedEvent(Id, amount));
    }
}
