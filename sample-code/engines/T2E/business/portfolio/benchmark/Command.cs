namespace Whycespace.Engines.T2E.Business.Portfolio.Benchmark;

public record BenchmarkCommand(
    string Action,
    string EntityId,
    object Payload
);
