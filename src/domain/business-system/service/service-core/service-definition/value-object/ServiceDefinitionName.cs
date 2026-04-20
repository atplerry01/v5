namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceDefinitionName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ServiceDefinitionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ServiceDefinitionName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ServiceDefinitionName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
