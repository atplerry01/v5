namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceCategory
{
    public const int MaxLength = 100;

    public string Value { get; }

    public ServiceCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceCategory must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ServiceCategory exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
