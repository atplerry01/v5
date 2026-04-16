using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Subject.Subject;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Subject.Subject;

[Authorize]
[ApiController]
[Route("api/economic/subject")]
[ApiExplorerSettings(GroupName = "economic.subject.subject")]
public sealed class SubjectController : ControllerBase
{
    private static readonly DomainRoute SubjectRoute = new("economic", "subject", "subject");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public SubjectController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _projectionsConnectionString = configuration.GetValue<string>("Projections:ConnectionString")
            ?? throw new InvalidOperationException(
                "Projections:ConnectionString is required. No fallback.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterSubject(
        [FromBody] ApiRequest<RegisterSubjectRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;

        var subjectId = _idGenerator.Generate(
            $"economic:subject:subject:{p.SubjectType}:{p.StructuralRefType}:{p.StructuralRefId}");

        var cmd = new RegisterEconomicSubjectCommand(
            subjectId,
            p.SubjectType,
            p.StructuralRefType,
            p.StructuralRefId,
            p.EconomicRefType,
            p.EconomicRefId);

        var result = await _dispatcher.DispatchAsync(cmd, SubjectRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck("economic_subject_registered"), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(
                "economic.subject.subject.register_failed",
                result.Error ?? "Unknown error",
                _clock.UtcNow));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubject(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_subject_subject.economic_subject_read_model " +
            "WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail(
                "economic.subject.subject.not_found",
                $"EconomicSubject {id} not found.",
                _clock.UtcNow));

        var stateJson = reader.GetString(0);
        var model = JsonSerializer.Deserialize<EconomicSubjectReadModel>(stateJson)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize EconomicSubjectReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), _clock.UtcNow));
    }
}

public sealed record RegisterSubjectRequestModel(
    string SubjectType,
    string StructuralRefType,
    string StructuralRefId,
    string EconomicRefType,
    string EconomicRefId);
