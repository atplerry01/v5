using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemReconciliation.SystemVerification;

[Authorize]
[ApiController]
[Route("api/control/system-verification")]
[ApiExplorerSettings(GroupName = "control.system-reconciliation.system-verification")]
public sealed class SystemVerificationController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-reconciliation", "system-verification");

    public SystemVerificationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("initiate")]
    public Task<IActionResult> Initiate([FromBody] ApiRequest<InitiateSystemVerificationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new InitiateSystemVerificationCommand(p.VerificationId, p.TargetSystem, p.InitiatedAt);
        return Dispatch(cmd, Route, "system_verification_initiated", "control.system-reconciliation.system-verification.initiate_failed", ct);
    }

    [HttpPost("pass")]
    public Task<IActionResult> Pass([FromBody] ApiRequest<PassSystemVerificationRequestModel> request, CancellationToken ct)
    {
        var cmd = new PassSystemVerificationCommand(request.Data.VerificationId, request.Data.PassedAt);
        return Dispatch(cmd, Route, "system_verification_passed", "control.system-reconciliation.system-verification.pass_failed", ct);
    }

    [HttpPost("fail")]
    public Task<IActionResult> Fail([FromBody] ApiRequest<FailSystemVerificationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FailSystemVerificationCommand(p.VerificationId, p.FailureReason, p.FailedAt);
        return Dispatch(cmd, Route, "system_verification_failed", "control.system-reconciliation.system-verification.fail_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<SystemVerificationReadModel>(
            id,
            "projection_control_system_reconciliation_system_verification",
            "system_verification_read_model",
            "control.system-reconciliation.system-verification.not_found",
            ct);
}

public sealed record InitiateSystemVerificationRequestModel(
    Guid VerificationId,
    string TargetSystem,
    DateTimeOffset InitiatedAt);

public sealed record PassSystemVerificationRequestModel(Guid VerificationId, DateTimeOffset PassedAt);

public sealed record FailSystemVerificationRequestModel(
    Guid VerificationId,
    string FailureReason,
    DateTimeOffset FailedAt);
