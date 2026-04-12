namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public readonly record struct RouteId
{
    public Guid Value { get; }

    public RouteId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RouteId value must not be empty.", nameof(value));

        Value = value;
    }
}
