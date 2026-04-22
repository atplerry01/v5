namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record CheckConsentResult(
    bool HasConsent,
    string? ConsentId,
    string? ConsentHash);
