namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public readonly record struct ExposureContext
{
    public Guid Value { get; }

    public ExposureContext(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExposureContext value must not be empty.", nameof(value));

        Value = value;
    }
}
