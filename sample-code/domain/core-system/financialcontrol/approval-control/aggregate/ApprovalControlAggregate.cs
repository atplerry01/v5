using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.ApprovalControl;

public sealed class ApprovalControlAggregate : AggregateRoot
{
    public string ControlName { get; private set; } = string.Empty;
    public decimal ThresholdAmount { get; private set; }
    public bool IsActive { get; private set; }

    public static ApprovalControlAggregate Create(Guid id, string controlName, decimal thresholdAmount)
    {
        var agg = new ApprovalControlAggregate
        {
            Id = id,
            ControlName = controlName,
            ThresholdAmount = thresholdAmount,
            IsActive = true
        };
        agg.RaiseDomainEvent(new ApprovalControlCreatedEvent(id, controlName, thresholdAmount));
        return agg;
    }

    public void Deactivate()
    {
        IsActive = false;
        RaiseDomainEvent(new ApprovalControlDeactivatedEvent(Id));
    }
}
