namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public readonly record struct ExposureLimit
{
    public decimal Value { get; }

    public ExposureLimit(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentException("ExposureLimit must be greater than zero.", nameof(value));

        Value = value;
    }
}
