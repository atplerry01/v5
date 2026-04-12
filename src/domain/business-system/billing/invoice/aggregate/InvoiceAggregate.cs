namespace Whycespace.Domain.BusinessSystem.Billing.Invoice;

public sealed class InvoiceAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<InvoiceLineItem> _lineItems = new();

    public InvoiceId Id { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public int Version { get; private set; }

    private InvoiceAggregate() { }

    public static InvoiceAggregate Create(InvoiceId id)
    {
        var aggregate = new InvoiceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new InvoiceCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddLineItem(InvoiceLineItem lineItem)
    {
        if (lineItem is null)
            throw new ArgumentNullException(nameof(lineItem));

        _lineItems.Add(lineItem);
    }

    public void Issue()
    {
        ValidateBeforeChange();

        var specification = new CanIssueSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw InvoiceErrors.InvalidStateTransition(Status, nameof(Issue));

        if (_lineItems.Count == 0)
            throw InvoiceErrors.LineItemRequired();

        var @event = new InvoiceIssuedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkAsPaid()
    {
        ValidateBeforeChange();

        var specification = new CanMarkAsPaidSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw InvoiceErrors.InvalidStateTransition(Status, nameof(MarkAsPaid));

        var @event = new InvoicePaidEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw InvoiceErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new InvoiceCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(InvoiceCreatedEvent @event)
    {
        Id = @event.InvoiceId;
        Status = InvoiceStatus.Draft;
        Version++;
    }

    private void Apply(InvoiceIssuedEvent @event)
    {
        Status = InvoiceStatus.Issued;
        Version++;
    }

    private void Apply(InvoicePaidEvent @event)
    {
        Status = InvoiceStatus.Paid;
        Version++;
    }

    private void Apply(InvoiceCancelledEvent @event)
    {
        Status = InvoiceStatus.Cancelled;
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
            throw InvoiceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw InvoiceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
