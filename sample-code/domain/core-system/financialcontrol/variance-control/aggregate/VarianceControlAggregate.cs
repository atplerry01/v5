using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.VarianceControl;

public sealed class VarianceControlAggregate : AggregateRoot
{
    public string ControlName { get; private set; } = string.Empty;
    public decimal TolerancePercent { get; private set; }

    public static VarianceControlAggregate Create(Guid id, string controlName, decimal tolerancePercent)
    {
        var agg = new VarianceControlAggregate
        {
            Id = id,
            ControlName = controlName,
            TolerancePercent = tolerancePercent
        };
        agg.RaiseDomainEvent(new VarianceControlCreatedEvent(id, controlName, tolerancePercent));
        return agg;
    }

    public void UpdateTolerance(decimal percent)
    {
        TolerancePercent = percent;
        RaiseDomainEvent(new VarianceControlToleranceUpdatedEvent(Id, percent));
    }
}
