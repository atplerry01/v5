namespace Whycespace.Engines.T2E.Constitutional.Policy.Rule;

public record PolicyRuleCommand(string Action, string EntityId, object Payload);
public sealed record CreatePolicyRuleCommand(string Id) : PolicyRuleCommand("Create", Id, null!);
