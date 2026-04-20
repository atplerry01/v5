namespace Whycespace.Domain.SharedKernel.Primitive.Money;

// Composes Amount + Currency. Both components validate themselves on construction
// (sentinel rejection on Amount, non-empty on Currency); Money adds no further
// invariants — accounting permits any sign/magnitude an Amount permits.
public readonly record struct Money(Amount Amount, Currency Currency);
