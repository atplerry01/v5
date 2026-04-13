namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record ValidateSessionCommand(string SessionId, string IdentityId, string? DeviceId);
