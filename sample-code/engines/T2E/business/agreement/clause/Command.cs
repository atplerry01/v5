namespace Whycespace.Engines.T2E.Business.Agreement.Clause;

public record ClauseCommand(
    string Action,
    string EntityId,
    object Payload
);
