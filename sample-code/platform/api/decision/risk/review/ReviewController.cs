using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Decision.Risk.Review;

public sealed class ReviewController
{
    private const string CommandPrefix = "decision.risk.review";

    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public ReviewController(DownstreamAdapter downstream, ProjectionAdapter projections)
    {
        _downstream = downstream;
        _projections = projections;
    }

    public Task<ApiResponse> CreateAsync(ReviewRequest request, ApiRequest context)
    {
        var entityId = request.EntityId ?? DeterministicIdHelper.FromSeed($"{context.RequestId}:{CommandPrefix}").ToString();

        return _downstream.SendCommandAsync(
            $"{CommandPrefix}.create",
            new { EntityId = entityId, request.Payload },
            context.RequestId,
            context.WhyceId,
            traceId: context.TraceId,
            aggregateId: entityId);
    }

    public async Task<ApiResponse> GetByIdAsync(string entityId, ApiRequest context)
    {
        var result = await _projections.QueryAsync<ReviewResponse>(
            CommandPrefix,
            new Dictionary<string, object> { ["aggregateId"] = entityId },
            context.TraceId);

        if (result.Data is null)
            return ApiResponse.NotFound($"Review {entityId} not found.", context.TraceId);

        return result;
    }
}
