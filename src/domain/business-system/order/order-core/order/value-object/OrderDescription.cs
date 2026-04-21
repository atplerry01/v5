using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

/// Typed structural descriptor for an order. Non-evolving, non-versioned —
/// captured once at creation time. NOT content (does not externalise to
/// `DocumentAggregate`).
public readonly record struct OrderDescription
{
    public string Value { get; }

    public OrderDescription(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "OrderDescription must not be empty.");
        Guard.Against(value!.Length > 512, "OrderDescription must not exceed 512 characters.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
