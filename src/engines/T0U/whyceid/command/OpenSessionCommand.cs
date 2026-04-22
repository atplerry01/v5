namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record OpenSessionCommand(
    string IdentityId,
    string? DeviceId,
    string SessionContext);
