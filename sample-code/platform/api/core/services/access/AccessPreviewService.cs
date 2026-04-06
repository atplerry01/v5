using Whycespace.Platform.Api.Core.Contracts.Access;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Services.Access;

/// <summary>
/// Runtime-delegated access preview service.
/// Sends preview requests to the runtime control plane for policy evaluation.
/// Returns advisory result — does NOT enforce or grant access.
/// If the preview fails, returns a DENY with explanation — never throws.
/// </summary>
public sealed class AccessPreviewService : IAccessPreviewService
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public AccessPreviewService(IRuntimeControlPlane controlPlane, IClock clock)
    {
        _controlPlane = controlPlane;
        _clock = clock;
    }

    public async Task<AccessDecisionPreviewView> PreviewAsync(
        Guid identityId,
        string resource,
        string action,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
            {
                CommandId = DeterministicIdHelper.FromSeed($"access-preview:{correlationId}:{identityId}:{resource}:{action}"),
                CommandType = "policy.access-preview",
                Payload = new { IdentityId = identityId.ToString(), Resource = resource, Action = action },
                CorrelationId = correlationId,
                Timestamp = _clock.UtcNowOffset
            });

            if (!result.Success)
            {
                return new AccessDecisionPreviewView
                {
                    Resource = resource,
                    Action = action,
                    Decision = "DENY",
                    Reasons = new List<string> { result.ErrorMessage ?? "Access denied by policy" }
                };
            }

            return new AccessDecisionPreviewView
            {
                Resource = resource,
                Action = action,
                Decision = "ALLOW",
                Reasons = new List<string> { "Access permitted" }
            };
        }
        catch
        {
            return new AccessDecisionPreviewView
            {
                Resource = resource,
                Action = action,
                Decision = "DENY",
                Reasons = new List<string> { "Access preview service unavailable" }
            };
        }
    }
}
