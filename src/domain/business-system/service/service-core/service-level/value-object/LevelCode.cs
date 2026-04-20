namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct LevelCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public LevelCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LevelCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"LevelCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
