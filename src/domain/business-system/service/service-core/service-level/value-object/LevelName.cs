namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct LevelName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public LevelName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LevelName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"LevelName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
