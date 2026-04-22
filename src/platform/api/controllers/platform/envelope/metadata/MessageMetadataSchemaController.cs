using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Platform.Envelope.Metadata;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Platform.Envelope.Metadata;

[Authorize]
[ApiController]
[Route("api/platform/envelope/metadata")]
[ApiExplorerSettings(GroupName = "platform.envelope.metadata")]
public sealed class MessageMetadataSchemaController : ControllerBase
{
    private static readonly DomainRoute Route = new("platform", "envelope", "metadata");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public MessageMetadataSchemaController(ISystemIntentDispatcher dispatcher, IIdGenerator idGenerator, IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("register")]
    public Task<IActionResult> Register([FromBody] ApiRequest<RegisterMessageMetadataSchemaRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = _idGenerator.Generate($"platform:envelope:metadata:schema:{p.SchemaVersion}");
        var cmd = new RegisterMessageMetadataSchemaCommand(id, p.SchemaVersion, p.RequiredFields, p.OptionalFields, _clock.UtcNow);
        return Dispatch(cmd, "message_metadata_schema_registered", "platform.envelope.metadata.register_failed", ct);
    }

    private async Task<IActionResult> Dispatch(object command, string ack, string failureCode, CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, Route, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow, result.CorrelationId));
    }
}

public sealed record RegisterMessageMetadataSchemaRequestModel(
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> OptionalFields);
