namespace Whyce.Engines.T0U.WhycePolicy.Result;

public sealed record PolicyEvaluationResult(
    bool IsCompliant,
    string DecisionHash,
    string ExecutionHash,
    string PolicyVersion,
    string[] RulesEvaluated,
    string? DenialReason);
