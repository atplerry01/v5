using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Classifies user intent into system routing coordinates.
/// Platform calls this to convert WhyceRequest → ClassifiedIntent.
///
/// MUST be deterministic: same input → same output.
/// MUST NOT call runtime, engines, or evaluate policy.
/// </summary>
public interface IIntentClassifierService
{
    Task<IntentClassificationResult> ClassifyAsync(
        WhyceRequest request,
        string correlationId,
        string? traceId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of intent classification.
/// Either succeeds with a ClassifiedIntent or fails with a reason.
/// </summary>
public sealed record IntentClassificationResult
{
    public bool Success { get; init; }
    public ClassifiedIntent? Intent { get; init; }
    public string? FailureReason { get; init; }
    public string? FailureCode { get; init; }

    public static IntentClassificationResult Ok(ClassifiedIntent intent) => new()
    {
        Success = true,
        Intent = intent
    };

    public static IntentClassificationResult Fail(string reason, string code) => new()
    {
        Success = false,
        FailureReason = reason,
        FailureCode = code
    };
}
