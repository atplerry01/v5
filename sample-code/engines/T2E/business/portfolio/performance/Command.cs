namespace Whycespace.Engines.T2E.Business.Portfolio.Performance;

public record PerformanceCommand(
    string Action,
    string EntityId,
    object Payload
);
