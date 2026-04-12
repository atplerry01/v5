namespace Whycespace.Domain.BusinessSystem.Billing.PaymentApplication;

public sealed class PaymentApplicationAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<PaymentAllocation> _allocations = new();

    public PaymentApplicationId Id { get; private set; }
    public PaymentApplicationStatus Status { get; private set; }
    public Guid InvoiceReference { get; private set; }
    public Guid PaymentSourceReference { get; private set; }
    public decimal OutstandingAmount { get; private set; }
    public IReadOnlyList<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public int Version { get; private set; }

    private PaymentApplicationAggregate() { }

    public static PaymentApplicationAggregate Create(PaymentApplicationId id, Guid invoiceReference, Guid paymentSourceReference, decimal outstandingAmount)
    {
        if (invoiceReference == Guid.Empty)
            throw PaymentApplicationErrors.MissingInvoiceReference();

        if (paymentSourceReference == Guid.Empty)
            throw PaymentApplicationErrors.MissingPaymentSource();

        var aggregate = new PaymentApplicationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new PaymentApplicationCreatedEvent(id);
        aggregate.Apply(@event, invoiceReference, paymentSourceReference, outstandingAmount);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddAllocation(PaymentAllocation allocation)
    {
        if (allocation is null)
            throw new ArgumentNullException(nameof(allocation));

        var totalAllocated = _allocations.Sum(a => a.Amount) + allocation.Amount;
        if (totalAllocated > OutstandingAmount)
            throw PaymentApplicationErrors.ExceedsOutstandingAmount(totalAllocated, OutstandingAmount);

        _allocations.Add(allocation);
    }

    public void ApplyPayment()
    {
        ValidateBeforeChange();

        var specification = new CanApplyPaymentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PaymentApplicationErrors.InvalidStateTransition(Status, nameof(ApplyPayment));

        if (_allocations.Count == 0)
            throw PaymentApplicationErrors.AllocationRequired();

        var @event = new PaymentApplicationAppliedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void ReverseApplication()
    {
        ValidateBeforeChange();

        var specification = new CanReverseApplicationSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PaymentApplicationErrors.InvalidStateTransition(Status, nameof(ReverseApplication));

        var @event = new PaymentApplicationReversedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PaymentApplicationCreatedEvent @event, Guid invoiceReference, Guid paymentSourceReference, decimal outstandingAmount)
    {
        Id = @event.PaymentApplicationId;
        Status = PaymentApplicationStatus.Pending;
        InvoiceReference = invoiceReference;
        PaymentSourceReference = paymentSourceReference;
        OutstandingAmount = outstandingAmount;
        Version++;
    }

    private void Apply(PaymentApplicationAppliedEvent @event)
    {
        Status = PaymentApplicationStatus.Applied;
        Version++;
    }

    private void Apply(PaymentApplicationReversedEvent @event)
    {
        Status = PaymentApplicationStatus.Reversed;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PaymentApplicationErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw PaymentApplicationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
