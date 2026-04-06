namespace Whycespace.Engines.T2E.Decision.Governance.Vote;

public record VoteCommand(string Action, string EntityId, object Payload);
public sealed record CreateVoteCommand(string Id) : VoteCommand("Create", Id, null!);
