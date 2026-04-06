using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.SpendControl;

public sealed class SpendControlAggregate : AggregateRoot
{
    public string ControlName { get; private set; } = string.Empty;
    public decimal SpendLimit { get; private set; }
    public bool IsSuspended { get; private set; }

    public static SpendControlAggregate Create(Guid id, string controlName, decimal spendLimit)
    {
        var agg = new SpendControlAggregate
        {
            Id = id,
            ControlName = controlName,
            SpendLimit = spendLimit,
            IsSuspended = false
        };
        agg.RaiseDomainEvent(new SpendControlCreatedEvent(id, controlName, spendLimit));
        return agg;
    }

    public void Suspend()
    {
        IsSuspended = true;
        RaiseDomainEvent(new SpendControlSuspendedEvent(Id));
    }
}
