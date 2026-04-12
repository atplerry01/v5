namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public readonly record struct UtilizationId
{
    public Guid Value { get; }

    public UtilizationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UtilizationId value must not be empty.", nameof(value));

        Value = value;
    }
}
