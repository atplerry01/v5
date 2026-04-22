namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record CheckConsentCommand(
    string IdentityId,
    string ConsentScope);
