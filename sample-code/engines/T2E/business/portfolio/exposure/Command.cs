namespace Whycespace.Engines.T2E.Business.Portfolio.Exposure;

public record ExposureCommand(
    string Action,
    string EntityId,
    object Payload
);
