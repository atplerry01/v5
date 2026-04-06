using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Routes a classified intent to the correct system path.
/// Resolves: ClassifiedIntent → IntentRoute (Cluster → Authority → SubCluster → WSS).
/// No direct engine calls — all routing targets WSS.
///
/// MUST NOT:
/// - Call runtime or engines
/// - Execute workflows
/// - Evaluate policy
/// </summary>
public interface IIntentRoutingService
{
    Task<IntentRoutingResult> ResolveRouteAsync(ClassifiedIntent intent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of intent routing resolution.
/// Either succeeds with an IntentRoute or fails with a reason.
/// </summary>
public sealed record IntentRoutingResult
{
    public bool Success { get; init; }
    public IntentRoute? Route { get; init; }
    public string? FailureReason { get; init; }
    public string? FailureCode { get; init; }

    public static IntentRoutingResult Ok(IntentRoute route) => new()
    {
        Success = true,
        Route = route
    };

    public static IntentRoutingResult Fail(string reason, string code) => new()
    {
        Success = false,
        FailureReason = reason,
        FailureCode = code
    };
}
