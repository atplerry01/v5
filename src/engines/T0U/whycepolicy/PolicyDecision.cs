namespace Whyce.Engines.T0U.WhycePolicy;

public sealed record PolicyDecision(
    bool IsAllowed,
    string DecisionHash);
