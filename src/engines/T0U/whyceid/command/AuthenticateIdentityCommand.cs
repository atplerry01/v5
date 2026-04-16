namespace Whycespace.Engines.T0U.WhyceId.Command;

/// <summary>
/// Input to <c>WhyceIdEngine.AuthenticateIdentity</c>. <c>Roles</c> is optional —
/// when supplied it carries the authenticated caller's already-resolved role
/// claims (typically extracted from the HTTP principal upstream); when null or
/// empty the engine falls back to its safe default. Adding the parameter as
/// optional preserves every existing caller verbatim.
/// </summary>
public sealed record AuthenticateIdentityCommand(
    string? Token,
    string? UserId,
    string? DeviceId,
    string[]? Roles = null);
