namespace Whycespace.Engines.T0U.WhyceId.IdentityGraph;

public sealed class IdentityGraphHandler : IIdentityGraphEngine
{
    public GraphDecisionResult Evaluate(GraphDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.SourceIdentityId == command.TargetIdentityId)
            return GraphDecisionResult.Deny("Cannot link an identity to itself.");

        if (command.LinkAlreadyExists)
            return GraphDecisionResult.Deny("Link already exists.");

        return GraphDecisionResult.Allow();
    }
}
