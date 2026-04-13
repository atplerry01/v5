using Whycespace.Engines.T0U.WhyceId.Model;

namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record AuthenticateIdentityResult(WhyceIdentity Identity, bool IsAuthenticated, string SessionId);
