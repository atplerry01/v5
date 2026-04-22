namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record RevokeConsentCommand(
    string ConsentId,
    string IdentityId);
