using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Deterministic intent classifier.
/// Maps WhyceRequest.IntentType → ClassifiedIntent using the IntentMappingRegistry.
/// Enriches metadata with structural context (identityId, correlationId, traceId, timestamp).
///
/// ZERO business logic. ZERO runtime calls. ZERO engine calls.
/// Pure static mapping + metadata enrichment.
/// </summary>
public sealed class IntentClassifierService : IIntentClassifierService
{
    private readonly IntentMappingRegistry _registry;
    private readonly IClock _clock;

    public IntentClassifierService(IntentMappingRegistry registry, IClock clock)
    {
        _registry = registry;
        _clock = clock;
    }

    public Task<IntentClassificationResult> ClassifyAsync(
        WhyceRequest request,
        string correlationId,
        string? traceId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IntentType))
            return Task.FromResult(IntentClassificationResult.Fail(
                "IntentType is required", "MISSING_INTENT_TYPE"));

        var mapping = _registry.Resolve(request.IntentType);

        if (mapping is null)
            return Task.FromResult(IntentClassificationResult.Fail(
                $"Unknown intent type: {request.IntentType}", "UNKNOWN_INTENT_TYPE"));

        if (string.IsNullOrWhiteSpace(mapping.WorkflowKey))
            return Task.FromResult(IntentClassificationResult.Fail(
                $"No workflow key mapped for intent: {request.IntentType}", "MISSING_WORKFLOW_KEY"));

        var metadata = BuildMetadata(request, correlationId, traceId);

        var classified = new ClassifiedIntent
        {
            Classification = mapping.Classification,
            Domain = mapping.Domain,
            WorkflowKey = mapping.WorkflowKey,
            Cluster = mapping.Cluster,
            Subcluster = mapping.Subcluster,
            Context = mapping.Context,
            Metadata = metadata
        };

        return Task.FromResult(IntentClassificationResult.Ok(classified));
    }

    private Dictionary<string, string> BuildMetadata(
        WhyceRequest request, string correlationId, string? traceId)
    {
        var metadata = new Dictionary<string, string>
        {
            ["identityId"] = request.IdentityId,
            ["correlationId"] = correlationId,
            ["timestamp"] = _clock.UtcNowOffset.ToString("O"),
            ["jurisdiction"] = request.Jurisdiction,
            ["intentType"] = request.IntentType
        };

        if (traceId is not null)
            metadata["traceId"] = traceId;

        if (!string.IsNullOrWhiteSpace(request.Intent))
            metadata["intentDescription"] = request.Intent;

        if (request.IntentData is not null)
        {
            foreach (var (key, value) in request.IntentData)
            {
                metadata[$"intentData.{key}"] = value;
            }
        }

        return metadata;
    }
}
