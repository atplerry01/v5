using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Transaction.Expense;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic;

[Authorize]
[ApiController]
[Route("api/transaction")]
[ApiExplorerSettings(GroupName = "economic.transaction.transaction")]
public sealed class TransactionController : ControllerBase
{
    private static readonly DomainRoute TransactionRoute = new("economic", "transaction", "transaction");
    private static readonly DomainRoute ExpenseRoute = new("economic", "transaction", "expense");

    // canonical kind tokens mirror TransactionKind constants in the domain.
    // we redeclare here to avoid importing the domain into the API layer.
    private const string KindExpense = "expense";

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public TransactionController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /api/transaction/initiate ───────────────────────────

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] ApiRequest<InitiateTransactionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var now = _clock.UtcNow;
        var seed = BuildTransactionSeed(p.Kind, p.References);
        var txnId = _idGenerator.Generate(seed);

        var refs = ToDtos(p.References);
        var command = new InitiateTransactionCommand(txnId, p.Kind, refs, now);

        var result = await _dispatcher.DispatchAsync(command, TransactionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new { transactionId = txnId }, now))
            : BadRequest(ApiResponse.Fail("economic.transaction.initiate_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/transaction/{id}/commit ────────────────────────

    [HttpPost("{transactionId:guid}/commit")]
    public async Task<IActionResult> Commit(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var command = new CommitTransactionCommand(transactionId, now);

        var result = await _dispatcher.DispatchAsync(command, TransactionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("transaction_committed"), now))
            : BadRequest(ApiResponse.Fail("economic.transaction.commit_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/transaction/{id}/fail ──────────────────────────

    [HttpPost("{transactionId:guid}/fail")]
    public async Task<IActionResult> Fail(
        Guid transactionId,
        [FromBody] ApiRequest<FailTransactionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var command = new FailTransactionCommand(transactionId, request.Data.Reason ?? string.Empty, now);

        var result = await _dispatcher.DispatchAsync(command, TransactionRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("transaction_failed"), now))
            : BadRequest(ApiResponse.Fail("economic.transaction.fail_failed", result.Error ?? "Unknown error", now));
    }

    // ── POST /api/transaction/record-expense ─────────────────────
    //
    // Orchestrated: Initiate -> RecordExpense -> Commit (or Fail).
    // This is the canonical entry point for expense recording in
    // production — it wraps the expense action in a transaction
    // envelope so that downstream ledger and capital domains can
    // react to TransactionCommittedEvent uniformly.

    [HttpPost("record-expense")]
    public async Task<IActionResult> RecordExpense(
        [FromBody] ApiRequest<RecordExpenseTransactionRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var initiatedAt = _clock.UtcNow;

        var expenseId = _idGenerator.Generate(
            $"economic:transaction:expense:{p.SourceReference}:{p.Amount}:{p.Currency}:{p.Category}");
        var txnId = _idGenerator.Generate(
            $"economic:transaction:transaction:expense:{expenseId}");

        var references = new List<TransactionReferenceDto> { new(KindExpense, expenseId) };

        var initiate = new InitiateTransactionCommand(txnId, KindExpense, references, initiatedAt);
        var initResult = await _dispatcher.DispatchAsync(initiate, TransactionRoute, cancellationToken);
        if (!initResult.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(
                "economic.transaction.record_expense.initiate_failed",
                initResult.Error ?? "Unknown error",
                initiatedAt));
        }

        var recordExpense = new RecordExpenseCommand(
            expenseId, p.Amount, p.Currency, p.Category, p.SourceReference);
        var recordResult = await _dispatcher.DispatchAsync(recordExpense, ExpenseRoute, cancellationToken);

        if (!recordResult.IsSuccess)
        {
            // compensate: fail the transaction envelope so downstream
            // consumers see a terminal Failed state, not a lingering Initiated.
            var failedAt = _clock.UtcNow;
            var fail = new FailTransactionCommand(
                txnId, recordResult.Error ?? "expense recording failed", failedAt);
            _ = await _dispatcher.DispatchAsync(fail, TransactionRoute, cancellationToken);

            return BadRequest(ApiResponse.Fail(
                "economic.transaction.record_expense.record_failed",
                recordResult.Error ?? "Unknown error",
                failedAt));
        }

        var committedAt = _clock.UtcNow;
        var commit = new CommitTransactionCommand(txnId, committedAt);
        var commitResult = await _dispatcher.DispatchAsync(commit, TransactionRoute, cancellationToken);

        if (!commitResult.IsSuccess)
        {
            return BadRequest(ApiResponse.Fail(
                "economic.transaction.record_expense.commit_failed",
                commitResult.Error ?? "Unknown error",
                committedAt));
        }

        return Ok(ApiResponse.Ok(
            new { transactionId = txnId, expenseId = expenseId, status = "committed" },
            committedAt));
    }

    // ── helpers ──────────────────────────────────────────────────

    private string BuildTransactionSeed(string kind, IReadOnlyList<TransactionReferenceRequestModel> references)
    {
        var parts = new List<string> { "economic:transaction:transaction", kind };
        foreach (var r in references) parts.Add($"{r.Kind}:{r.Id}");
        return string.Join(":", parts);
    }

    private static IReadOnlyList<TransactionReferenceDto> ToDtos(
        IReadOnlyList<TransactionReferenceRequestModel> references)
    {
        var dtos = new List<TransactionReferenceDto>(references.Count);
        foreach (var r in references) dtos.Add(new TransactionReferenceDto(r.Kind, r.Id));
        return dtos;
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record InitiateTransactionRequestModel(
    string Kind,
    IReadOnlyList<TransactionReferenceRequestModel> References);

public sealed record TransactionReferenceRequestModel(
    string Kind,
    Guid Id);

public sealed record FailTransactionRequestModel(string Reason);

public sealed record RecordExpenseTransactionRequestModel(
    decimal Amount,
    string Currency,
    string Category,
    string SourceReference);
