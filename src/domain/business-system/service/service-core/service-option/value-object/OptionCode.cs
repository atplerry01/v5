using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct OptionCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public OptionCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "OptionCode must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"OptionCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
