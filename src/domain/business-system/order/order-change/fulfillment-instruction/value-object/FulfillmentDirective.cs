using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct FulfillmentDirective
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public FulfillmentDirective(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "FulfillmentDirective must not be empty.");
        Guard.Against(value!.Length > MaxLength, $"FulfillmentDirective exceeds {MaxLength} characters.");

        Value = value;
    }
}
