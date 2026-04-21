using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public readonly record struct ConstraintDescriptor
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public ConstraintDescriptor(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ConstraintDescriptor must not be empty.");
        Guard.Against(value!.Length > MaxLength, $"ConstraintDescriptor exceeds {MaxLength} characters.");

        Value = value;
    }
}
