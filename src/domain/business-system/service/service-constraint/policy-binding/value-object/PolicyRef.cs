using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public readonly record struct PolicyRef
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PolicyRef(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "PolicyRef must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"PolicyRef exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
