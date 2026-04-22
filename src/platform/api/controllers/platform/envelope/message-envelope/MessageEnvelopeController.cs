using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Envelope.MessageEnvelope;

[Authorize]
[ApiController]
[Route("api/platform/envelope/message-envelope")]
[ApiExplorerSettings(GroupName = "platform.envelope.message_envelope")]
public sealed class MessageEnvelopeController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "envelope", "message-envelope");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public MessageEnvelopeController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException("Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateMessageEnvelopeRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:envelope:message-envelope:{p.HeaderTraceId}:{p.HeaderSpanId}");
        var cmd = new CreateMessageEnvelopeCommand(
            id, p.HeaderMessageId, p.HeaderContentType, p.HeaderMessageKindHint,
            p.HeaderSourceClassification, p.HeaderSourceContext, p.HeaderSourceDomain,
            p.HeaderDestinationClassification, p.HeaderDestinationContext, p.HeaderDestinationDomain,
            p.HeaderTraceId, p.HeaderSpanId, p.HeaderParentSpanId, p.HeaderSamplingFlag,
            p.PayloadTypeRef, p.PayloadEncoding, p.PayloadSchemaRef, p.PayloadBytes,
            p.MetadataCorrelationId, p.MetadataCausationId, p.MetadataMessageVersion,
            _clock.UtcNow, p.MetadataTenantId, p.MessageKind, _clock.UtcNow);
        return Dispatch(cmd, "message_envelope_created", "platform.envelope.message_envelope.create_failed", ct);
    }

    [HttpPost("dispatch")]
    public Task<IActionResult> Dispatch([FromBody] ApiRequest<DispatchMessageEnvelopeRequestModel> request, CancellationToken ct)
    {
        var cmd = new DispatchMessageEnvelopeCommand(request.Data.EnvelopeId, _clock.UtcNow);
        return Dispatch(cmd, "message_envelope_dispatched", "platform.envelope.message_envelope.dispatch_failed", ct);
    }

    [HttpPost("acknowledge")]
    public Task<IActionResult> Acknowledge([FromBody] ApiRequest<AcknowledgeMessageEnvelopeRequestModel> request, CancellationToken ct)
    {
        var cmd = new AcknowledgeMessageEnvelopeCommand(request.Data.EnvelopeId, _clock.UtcNow);
        return Dispatch(cmd, "message_envelope_acknowledged", "platform.envelope.message_envelope.acknowledge_failed", ct);
    }

    [HttpPost("reject")]
    public Task<IActionResult> Reject([FromBody] ApiRequest<RejectMessageEnvelopeRequestModel> request, CancellationToken ct)
    {
        var cmd = new RejectMessageEnvelopeCommand(request.Data.EnvelopeId, request.Data.RejectionReason, _clock.UtcNow);
        return Dispatch(cmd, "message_envelope_rejected", "platform.envelope.message_envelope.reject_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_platform_envelope_message_envelope.message_envelope_read_model WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail("platform.envelope.message_envelope.not_found", $"MessageEnvelope {id} not found.", _clock.UtcNow));
        var model = JsonSerializer.Deserialize<MessageEnvelopeReadModel>(reader.GetString(0))
            ?? throw new InvalidOperationException($"Failed to deserialize MessageEnvelopeReadModel for {id}.");
        return Ok(ApiResponse.Ok(model, RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }

    private Guid RequestCorrelationId()
    {
        if (HttpContext is { } ctx
            && ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var values)
            && Guid.TryParse(values.ToString(), out var parsed))
            return parsed;
        return Guid.Empty;
    }
}

public sealed record CreateMessageEnvelopeRequestModel(
    Guid HeaderMessageId,
    string HeaderContentType,
    string HeaderMessageKindHint,
    string HeaderSourceClassification,
    string HeaderSourceContext,
    string HeaderSourceDomain,
    string? HeaderDestinationClassification,
    string? HeaderDestinationContext,
    string? HeaderDestinationDomain,
    string HeaderTraceId,
    string HeaderSpanId,
    string? HeaderParentSpanId,
    bool HeaderSamplingFlag,
    string PayloadTypeRef,
    string PayloadEncoding,
    string? PayloadSchemaRef,
    byte[] PayloadBytes,
    Guid MetadataCorrelationId,
    Guid MetadataCausationId,
    int MetadataMessageVersion,
    Guid? MetadataTenantId,
    string MessageKind);

public sealed record DispatchMessageEnvelopeRequestModel(Guid EnvelopeId);
public sealed record AcknowledgeMessageEnvelopeRequestModel(Guid EnvelopeId);
public sealed record RejectMessageEnvelopeRequestModel(Guid EnvelopeId, string RejectionReason);
