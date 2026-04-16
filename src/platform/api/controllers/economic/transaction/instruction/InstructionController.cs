using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Instruction;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Transaction.Instruction;

[Authorize]
[ApiController]
[Route("api/instruction")]
[ApiExplorerSettings(GroupName = "economic.transaction.instruction")]
public sealed class InstructionController : ControllerBase
{
    private static readonly DomainRoute InstructionRoute = new("economic", "transaction", "instruction");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public InstructionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/instruction/create ─────────────────────────────

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] ApiRequest<CreateInstructionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var instructionId = _idGenerator.Generate(
            $"economic:transaction:instruction:{p.FromAccountId}:{p.ToAccountId}:{p.Amount}:{p.Currency}:{p.Type}");

        var command = new CreateInstructionCommand(
            instructionId,
            p.FromAccountId,
            p.ToAccountId,
            p.Amount,
            p.Currency,
            p.Type,
            now);

        var result = await _dispatcher.DispatchAsync(command, InstructionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new { instructionId }, now))
            : BadRequest(ApiResponse.Fail("economic.instruction.create_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/instruction/{id}/execute ───────────────────────

    [HttpPost("{instructionId:guid}/execute")]
    public async Task<IActionResult> Execute(
        Guid instructionId,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var command = new ExecuteInstructionCommand(instructionId, now);

        var result = await _dispatcher.DispatchAsync(command, InstructionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("instruction_executed"), now))
            : BadRequest(ApiResponse.Fail("economic.instruction.execute_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/instruction/{id}/cancel ────────────────────────

    [HttpPost("{instructionId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid instructionId,
        [FromBody] ApiRequest<CancelInstructionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var command = new CancelInstructionCommand(instructionId, request.Data.Reason ?? string.Empty, now);

        var result = await _dispatcher.DispatchAsync(command, InstructionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("instruction_cancelled"), now))
            : BadRequest(ApiResponse.Fail("economic.instruction.cancel_failed", result.Error ?? "Unknown error", now));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record CreateInstructionRequestModel(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    string Type);

public sealed record CancelInstructionRequestModel(string Reason);
