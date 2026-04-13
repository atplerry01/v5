namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record AuthenticateIdentityCommand(string? Token, string? UserId, string? DeviceId);
