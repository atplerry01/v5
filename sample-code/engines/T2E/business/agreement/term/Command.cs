namespace Whycespace.Engines.T2E.Business.Agreement.Term;

public record TermCommand(
    string Action,
    string EntityId,
    object Payload
);
