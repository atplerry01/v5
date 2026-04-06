using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Stub implementation of WhyceID adapter.
/// Delegates identity resolution to the runtime control plane via T0U commands.
/// Deterministic — no random data. All IDs derived from input seeds.
///
/// In production, this will be replaced by a runtime-backed implementation
/// that calls T0U WhyceID engine through the standard command pipeline.
/// </summary>
public sealed class WhyceIdServiceStub : IWhyceIdService
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public WhyceIdServiceStub(IRuntimeControlPlane controlPlane, IClock clock)
    {
        _controlPlane = controlPlane;
        _clock = clock;
    }

    public async Task<WhyceIdentity?> ResolveAsync(string whyceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(whyceId))
            return null;

        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = DeterministicIdHelper.FromSeed($"whyceid.resolve:{whyceId}"),
            CommandType = "whyceid.resolve-profile",
            Payload = new { WhyceId = whyceId },
            CorrelationId = whyceId,
            Timestamp = _clock.UtcNowOffset,
            WhyceId = whyceId
        });

        if (!result.Success)
            return null;

        var identityId = DeterministicIdHelper.FromSeed($"identity:{whyceId}");

        return new WhyceIdentity
        {
            IdentityId = identityId,
            Roles = new List<string> { "user" },
            Attributes = new Dictionary<string, string>
            {
                ["jurisdiction"] = "default",
                ["tier"] = "standard"
            },
            TrustScore = 0.75m,
            Consents = new List<string> { "platform.access", "data.processing" },
            IsVerified = true,
            SessionId = DeterministicIdHelper.FromSeed($"session:{whyceId}:{_clock.UtcNow:yyyyMMdd}").ToString("N")
        };
    }

    public async Task<bool> ValidateSessionAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = DeterministicIdHelper.FromSeed($"whyceid.session-validate:{identityId}"),
            CommandType = "whyceid.validate-session",
            Payload = new { IdentityId = identityId.ToString() },
            CorrelationId = identityId.ToString(),
            Timestamp = _clock.UtcNowOffset
        });

        return result.Success;
    }
}
