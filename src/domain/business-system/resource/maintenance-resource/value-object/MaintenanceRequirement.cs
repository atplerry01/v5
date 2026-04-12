namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public readonly record struct MaintenanceRequirement
{
    public string Value { get; }

    public MaintenanceRequirement(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("MaintenanceRequirement must not be empty.", nameof(value));

        Value = value;
    }
}
