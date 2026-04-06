using Whyce.Engines.T0U.WhyceId.Model;

namespace Whyce.Engines.T0U.WhyceId.Result;

public sealed record ResolveIdentityResult(WhyceIdentity Identity, bool IsResolved);
