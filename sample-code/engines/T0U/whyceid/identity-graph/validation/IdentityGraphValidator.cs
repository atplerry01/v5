namespace Whycespace.Engines.T0U.WhyceId.IdentityGraph;

public sealed class IdentityGraphValidator
{
    public IdentityValidationResult Validate(GraphDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SourceIdentityId))
            return IdentityValidationResult.Invalid("SourceIdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.TargetIdentityId))
            return IdentityValidationResult.Invalid("TargetIdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.LinkType))
            return IdentityValidationResult.Invalid("LinkType is required.");
        return IdentityValidationResult.Valid();
    }
}
