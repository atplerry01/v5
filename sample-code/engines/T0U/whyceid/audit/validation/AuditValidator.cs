namespace Whycespace.Engines.T0U.WhyceId.Audit;

public sealed class AuditValidator
{
    public IdentityValidationResult Validate(AuditDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ActorId))
            return IdentityValidationResult.Invalid("ActorId is required.");
        if (string.IsNullOrWhiteSpace(command.Action))
            return IdentityValidationResult.Invalid("Action is required.");
        return IdentityValidationResult.Valid();
    }
}
