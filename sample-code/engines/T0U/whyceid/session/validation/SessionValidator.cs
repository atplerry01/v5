namespace Whycespace.Engines.T0U.WhyceId.Session;

public sealed class SessionValidator
{
    public IdentityValidationResult Validate(SessionDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.DeviceId))
            return IdentityValidationResult.Invalid("DeviceId is required.");
        return IdentityValidationResult.Valid();
    }
}
