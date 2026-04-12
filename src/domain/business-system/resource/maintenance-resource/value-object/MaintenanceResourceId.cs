namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public readonly record struct MaintenanceResourceId
{
    public Guid Value { get; }

    public MaintenanceResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MaintenanceResourceId value must not be empty.", nameof(value));

        Value = value;
    }
}
