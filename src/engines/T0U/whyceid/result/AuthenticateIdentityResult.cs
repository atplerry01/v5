using Whyce.Engines.T0U.WhyceId.Model;

namespace Whyce.Engines.T0U.WhyceId.Result;

public sealed record AuthenticateIdentityResult(WhyceIdentity Identity, bool IsAuthenticated, string SessionId);
