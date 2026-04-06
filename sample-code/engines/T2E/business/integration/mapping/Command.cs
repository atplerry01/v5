namespace Whycespace.Engines.T2E.Business.Integration.Mapping;

public record MappingCommand(
    string Action,
    string EntityId,
    object Payload
);
