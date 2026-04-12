namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public readonly record struct MandateName
{
    public string Value { get; }

    public MandateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MandateName must not be empty.", nameof(value));

        Value = value;
    }
}
