namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct OptionName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public OptionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OptionName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"OptionName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
