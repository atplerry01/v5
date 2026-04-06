namespace Whycespace.Engines.T2E.Business.Integration.Transformation;

public record TransformationCommand(
    string Action,
    string EntityId,
    object Payload
);
