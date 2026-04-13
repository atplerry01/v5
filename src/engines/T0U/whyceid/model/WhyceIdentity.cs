using Whycespace.Engines.T0U.WhyceId.Consent;
using Whycespace.Engines.T0U.WhyceId.Verification;

namespace Whycespace.Engines.T0U.WhyceId.Model;

public sealed record WhyceIdentity(
    string IdentityId,
    string[] Roles,
    IdentityAttribute[] Attributes,
    int TrustScore,
    VerificationStatus VerificationStatus,
    string SessionId,
    string? DeviceId,
    ConsentRecord[] Consents);
