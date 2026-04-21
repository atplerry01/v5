using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct LevelCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public LevelCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "LevelCode must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"LevelCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
