namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct OptionCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public OptionCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OptionCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"OptionCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
