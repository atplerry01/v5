namespace Whycespace.Engines.T0U.WhyceId.Command;

public sealed record GrantConsentCommand(
    string IdentityId,
    string ConsentScope,
    string ConsentPurpose);
