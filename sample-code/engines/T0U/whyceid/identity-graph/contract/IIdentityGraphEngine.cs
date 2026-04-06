namespace Whycespace.Engines.T0U.WhyceId.IdentityGraph;

public interface IIdentityGraphEngine
{
    GraphDecisionResult Evaluate(GraphDecisionCommand command);
}

public sealed record GraphDecisionCommand(
    string SourceIdentityId,
    string TargetIdentityId,
    string LinkType,
    bool LinkAlreadyExists);

public sealed record GraphDecisionResult(
    bool CanLink,
    string? Reason = null)
{
    public static GraphDecisionResult Allow() => new(true);
    public static GraphDecisionResult Deny(string reason) => new(false, reason);
}
