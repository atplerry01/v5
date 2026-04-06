namespace Whycespace.Engines.T2E.Decision.Governance.Authority;

public record AuthorityCommand(
    string Action,
    string EntityId,
    object Payload
);
