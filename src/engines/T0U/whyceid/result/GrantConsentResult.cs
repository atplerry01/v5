namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record GrantConsentResult(
    string ConsentId,
    string ConsentHash,
    bool IsGranted);
