using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public AssignmentScope(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "AssignmentScope must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"AssignmentScope exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
