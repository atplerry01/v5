namespace Whycespace.Engines.T0U.WhyceId.Device;

public sealed class DeviceValidator
{
    public IdentityValidationResult Validate(DeviceDecisionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.IdentityId))
            return IdentityValidationResult.Invalid("IdentityId is required.");
        if (string.IsNullOrWhiteSpace(command.DeviceFingerprint))
            return IdentityValidationResult.Invalid("DeviceFingerprint is required.");
        return IdentityValidationResult.Valid();
    }
}
