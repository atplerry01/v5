namespace Whycespace.Engines.T0U.WhycePolicy.Core;

public abstract record PolicyCommand;

public sealed record EvaluatePolicyCommand(string PolicyId, string SubjectId, string Context) : PolicyCommand;
public sealed record SimulatePolicyCommand(string PolicyId, string ScenarioId) : PolicyCommand;
public sealed record ValidatePolicyRuleCreateCommand(string RuleId) : PolicyCommand;
public sealed record ValidatePolicyEnforcementCreateCommand(string EnforcementId) : PolicyCommand;
public sealed record EnforcePolicyCommand(string CommandType, bool PolicyPassed, string? PolicyId) : PolicyCommand;
