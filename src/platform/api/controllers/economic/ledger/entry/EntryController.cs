using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Ledger.Entry;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Ledger.Entry;

/// <summary>
/// Read-only controller for the ledger/entry read model.
///
/// Entry has no standalone command surface (see <c>LedgerPolicyModule</c> and
/// <c>LedgerEntryApplicationModule</c> XML remarks). Entries are created as a
/// side effect of journal posting. This controller therefore exposes only a
/// GET against <c>projection_economic_ledger_entry.entry_read_model</c>, using
/// the same inline Npgsql query shape as <c>CapitalControllerBase.LoadReadModel</c>
/// without inheriting it (ledger context does not depend on capital-context types).
/// </summary>
[Authorize]
[ApiController]
[Route("api/economic/ledger/entry")]
[ApiExplorerSettings(GroupName = "economic.ledger.entry")]
public sealed class EntryController : ControllerBase
{
    private static readonly JsonSerializerOptions ReadModelJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public EntryController(IClock clock, IConfiguration configuration)
    {
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEntry(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_ledger_entry.entry_read_model WHERE aggregate_id = @id LIMIT 1",
            conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail(
                "economic.ledger.entry.not_found",
                $"LedgerEntry {id} not found.",
                _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<EntryReadModel>(stateJson, ReadModelJson)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize EntryReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), _clock.UtcNow));
    }
}
