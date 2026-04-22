using Whycespace.Engines.T0U.WhyceId.Model;

namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record BuildAuthorizationContextResult(
    AuthorizationContext Context,
    bool IsAuthorizable,
    string? DenialReason);
