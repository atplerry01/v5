namespace Whycespace.Engines.T2E.Decision.Risk.Exposure;

public record ExposureCommand(
    string Action,
    string EntityId,
    object Payload
);
