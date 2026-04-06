using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Operational.Global.IncidentResponse;

public sealed class IncidentController
{
    private const string CommandPrefix = "operational.incident";
    private const string ProjectionName = "operational.incident";

    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public IncidentController(DownstreamAdapter downstream, ProjectionAdapter projections)
    {
        _downstream = downstream;
        _projections = projections;
    }

    public Task<ApiResponse> CreateAsync(CreateIncidentDto dto, ApiRequest context) =>
        _downstream.SendCommandAsync($"{CommandPrefix}.create",
            new { AggregateId = dto.IncidentId ?? DeterministicIdHelper.FromSeed($"{context.RequestId}:{CommandPrefix}").ToString(), IncidentType = dto.Type, dto.Severity, Source = dto.Source ?? "platform" },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: dto.IncidentId);

    public Task<ApiResponse> AssignAsync(AssignIncidentDto dto, ApiRequest context) =>
        _downstream.SendCommandAsync($"{CommandPrefix}.assign", new { dto.AggregateId, dto.AssigneeIdentityId, dto.EscalationLevel },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: dto.AggregateId);

    public Task<ApiResponse> ResolveAsync(ResolveIncidentDto dto, ApiRequest context) =>
        _downstream.SendCommandAsync($"{CommandPrefix}.resolve", new { dto.AggregateId },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: dto.AggregateId);

    public Task<ApiResponse> CloseAsync(CloseIncidentDto dto, ApiRequest context) =>
        _downstream.SendCommandAsync($"{CommandPrefix}.close", new { dto.AggregateId },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: dto.AggregateId);

    public async Task<ApiResponse> GetByIdAsync(string incidentId, ApiRequest context)
    {
        var result = await _projections.QueryAsync<IncidentQueryResponse>(ProjectionName,
            new Dictionary<string, object> { ["aggregateId"] = incidentId }, context.TraceId);
        if (result.Data is null) return ApiResponse.NotFound($"Incident {incidentId} not found.", context.TraceId);
        return result;
    }
}
