using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Business.Billing.PaymentApplication;

public sealed class PaymentApplicationController
{
    private const string CommandPrefix = "business.billing.payment-application";

    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public PaymentApplicationController(DownstreamAdapter downstream, ProjectionAdapter projections)
    {
        _downstream = downstream;
        _projections = projections;
    }

    public Task<ApiResponse> CreateAsync(PaymentApplicationRequest request, ApiRequest context)
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
        var result = await _projections.QueryAsync<PaymentApplicationResponse>(
            CommandPrefix,
            new Dictionary<string, object> { ["aggregateId"] = entityId },
            context.TraceId);

        if (result.Data is null)
            return ApiResponse.NotFound($"PaymentApplication {entityId} not found.", context.TraceId);

        return result;
    }
}
