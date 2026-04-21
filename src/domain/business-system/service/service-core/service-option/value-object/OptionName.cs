using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct OptionName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public OptionName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "OptionName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"OptionName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
