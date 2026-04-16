using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Reconciliation.Process;

[Authorize]
[ApiController]
[Route("api/economic/reconciliation/process")]
[ApiExplorerSettings(GroupName = "economic.reconciliation.process")]
public sealed class ProcessController : ControllerBase
{
    private static readonly DomainRoute ProcessRoute = new("economic", "reconciliation", "process");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public ProcessController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(
        [FromBody] ApiRequest<TriggerReconciliationRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var processId = _idGenerator.Generate(
            $"economic:reconciliation:process:{p.LedgerReference}:{p.ObservedReference}");

        var command = new TriggerReconciliationCommand(
            processId,
            p.LedgerReference,
            p.ObservedReference,
            _clock.UtcNow);

        var result = await _dispatcher.DispatchAsync(command, ProcessRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("reconciliation_triggered"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.process.trigger_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("matched")]
    public async Task<IActionResult> Matched(
        [FromBody] ApiRequest<ProcessIdRequestModel> request,
        CancellationToken cancellationToken)
    {
        var command = new MarkMatchedCommand(request.Data.ProcessId);
        var result = await _dispatcher.DispatchAsync(command, ProcessRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("reconciliation_matched"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.process.matched_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("mismatched")]
    public async Task<IActionResult> Mismatched(
        [FromBody] ApiRequest<ProcessIdRequestModel> request,
        CancellationToken cancellationToken)
    {
        var command = new MarkMismatchedCommand(request.Data.ProcessId);
        var result = await _dispatcher.DispatchAsync(command, ProcessRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("reconciliation_mismatched"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.process.mismatched_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve(
        [FromBody] ApiRequest<ProcessIdRequestModel> request,
        CancellationToken cancellationToken)
    {
        var command = new ResolveReconciliationCommand(request.Data.ProcessId);
        var result = await _dispatcher.DispatchAsync(command, ProcessRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("reconciliation_resolved"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.reconciliation.process.resolve_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

public sealed record TriggerReconciliationRequestModel(
    Guid LedgerReference,
    Guid ObservedReference);

public sealed record ProcessIdRequestModel(Guid ProcessId);
