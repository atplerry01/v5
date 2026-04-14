using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic;

[Authorize]
[ApiController]
[Route("api/expense")]
[ApiExplorerSettings(GroupName = "economic.transaction.expense")]
public sealed class ExpenseController : ControllerBase
{
    private static readonly DomainRoute ExpenseRoute = new("economic", "transaction", "expense");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public ExpenseController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/expense/record ─────────────────────────────────

    [HttpPost("record")]
    public async Task<IActionResult> RecordExpense(
        [FromBody] ApiRequest<RecordExpenseRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var expenseId = _idGenerator.Generate(
            $"economic:transaction:expense:{p.SourceReference}:{p.Amount}:{p.Currency}:{p.Category}");

        var command = new RecordExpenseCommand(
            expenseId,
            p.Amount,
            p.Currency,
            p.Category,
            p.SourceReference);

        var result = await _dispatcher.DispatchAsync(command, ExpenseRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("expense_recorded"), _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.expense.record_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record RecordExpenseRequestModel(
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference);
