namespace Whycespace.Engines.T2E.Business.Integration.Job;

public record JobCommand(
    string Action,
    string EntityId,
    object Payload
);
