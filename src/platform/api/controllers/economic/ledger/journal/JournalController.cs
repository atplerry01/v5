using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Journal;

[Authorize]
[ApiController]
[Route("api/ledger")]
[ApiExplorerSettings(GroupName = "economic.ledger.journal")]
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

    // ── POST /api/ledger/post ────────────────────────────────────

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
                "economic.ledger.post_failed",
                result.Error ?? "Unknown error",
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
