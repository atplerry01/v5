using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Journal;

/// <summary>
/// Phase 4.5 T4.5.2 — restricted operator surface for direct ledger journal
/// posting. Originally exposed at <c>/api/ledger/post</c> as a general API;
/// that path was a control-plane bypass (Phase 4 Finding 12) because it
/// allowed any authenticated caller to post journals without the per-account
/// limit gate enforced by the transaction lifecycle workflow.
///
/// Hardening (Phase 4.5):
/// 1. **Route relocated** to <c>/internal/ledger/post</c> to signal non-public surface.
/// 2. **Authorization tightened** to admin role only.
/// 3. **Handler-level enforcement (T4.5.3)**: even an authenticated admin
///    request is rejected by <c>PostJournalEntriesHandler</c>, which now
///    requires <c>context.IsSystem == true</c>. Only the transaction
///    lifecycle (<c>PostToLedgerStep</c>) and the payout pipeline
///    (<c>PostLedgerJournalStep</c>) set that flag via
///    <c>ISystemIntentDispatcher.DispatchSystemAsync</c>. The controller's
///    <c>DispatchAsync</c> path observes <c>IsSystem == false</c> and is
///    rejected at the engine boundary.
///
/// Net effect: this endpoint is unreachable for ledger mutation under the
/// current dispatcher contract. It remains in the codebase as an explicit
/// admin escape-hatch SHELL — to actually re-enable operator corrections,
/// a future change must (a) introduce a dedicated operator-origin
/// dispatcher path that sets a discrete flag AND (b) extend the handler
/// gate to honor it. Both gates intentionally stay closed by default.
/// </summary>
[Authorize(Roles = "admin")]
[ApiController]
[Route("internal/ledger")]
[ApiExplorerSettings(GroupName = "internal.economic.ledger.journal")]
public sealed class JournalController : ControllerBase
{
    private static readonly DomainRoute JournalRoute = new("economic", "ledger", "journal");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public JournalController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    // ── POST /internal/ledger/post ─────────────────────────────────────
    //
    // Direct ledger entry post. Restricted to admin role at the API layer
    // and rejected at the handler layer because DispatchAsync does not set
    // IsSystem. To use this endpoint for genuine operator corrections, a
    // future change must extend the dispatcher / handler contract — see
    // the class comment above.

    [HttpPost("post")]
    public async Task<IActionResult> PostJournalEntries(
        [FromBody] ApiRequest<PostJournalEntriesRequestModel> request,
        CancellationToken cancellationToken)
    {
        var p = request.Data;
        var seed = BuildSeed(p);
        var journalId = _idGenerator.Generate($"economic:ledger:journal:{seed}");

        var entries = new List<JournalEntryInput>(p.Entries.Count);
        foreach (var e in p.Entries)
        {
            var entryId = _idGenerator.Generate(
                $"economic:ledger:entry:{journalId}:{e.AccountId}:{e.Amount}:{e.Direction}");
            entries.Add(new JournalEntryInput(
                entryId,
                e.AccountId,
                e.Amount,
                e.Currency,
                e.Direction));
        }

        var command = new PostJournalEntriesCommand(p.LedgerId, journalId, entries);

        var result = await _dispatcher.DispatchAsync(command, JournalRoute, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("journal_entries_posted"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.ledger.post_rejected",
                result.Error ?? "Direct ledger post rejected by control-plane origin gate.",
                _clock.UtcNow));
    }

    private static string BuildSeed(PostJournalEntriesRequestModel p)
    {
        var parts = new List<string> { p.LedgerId.ToString(), p.Reference };
        foreach (var e in p.Entries)
            parts.Add($"{e.AccountId}:{e.Amount}:{e.Currency}:{e.Direction}");
        return string.Join("|", parts);
    }
}

// ── Request models ──────────────────────────────────────────────

public sealed record PostJournalEntriesRequestModel(
    Guid LedgerId,
    string Reference,
    IReadOnlyList<JournalEntryRequestModel> Entries);

public sealed record JournalEntryRequestModel(
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction);
