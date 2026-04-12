namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed record BenchmarkCreatedEvent(
    BenchmarkId BenchmarkId,
    BenchmarkName BenchmarkName);
