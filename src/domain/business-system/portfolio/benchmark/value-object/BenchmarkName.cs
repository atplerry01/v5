namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public readonly record struct BenchmarkName
{
    public string Value { get; }

    public BenchmarkName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("BenchmarkName must not be empty.", nameof(value));

        Value = value;
    }
}
