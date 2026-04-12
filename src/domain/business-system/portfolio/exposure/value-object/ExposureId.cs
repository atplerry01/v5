namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public readonly record struct ExposureId
{
    public Guid Value { get; }

    public ExposureId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ExposureId value must not be empty.", nameof(value));

        Value = value;
    }
}
