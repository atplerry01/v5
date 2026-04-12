namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public readonly record struct BenchmarkId
{
    public Guid Value { get; }

    public BenchmarkId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BenchmarkId value must not be empty.", nameof(value));

        Value = value;
    }
}
