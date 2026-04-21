using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct LevelName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public LevelName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "LevelName must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"LevelName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
