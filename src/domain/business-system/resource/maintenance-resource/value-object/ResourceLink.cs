namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public readonly record struct ResourceLink
{
    public Guid Value { get; }

    public ResourceLink(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ResourceLink value must not be empty.", nameof(value));

        Value = value;
    }
}
