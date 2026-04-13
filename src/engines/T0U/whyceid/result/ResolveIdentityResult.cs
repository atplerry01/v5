using Whycespace.Engines.T0U.WhyceId.Model;

namespace Whycespace.Engines.T0U.WhyceId.Result;

public sealed record ResolveIdentityResult(WhyceIdentity Identity, bool IsResolved);
