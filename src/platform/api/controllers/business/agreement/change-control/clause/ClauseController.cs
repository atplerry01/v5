using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Clause;

[Authorize]
[ApiController]
[Route("api/change-control/clause")]
[ApiExplorerSettings(GroupName = "business.agreement.change-control.clause")]
public sealed class ClauseController : BusinessControllerBase
{
    private static readonly DomainRoute ClauseRoute = new("business", "agreement", "clause");

    public ClauseController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateClauseRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreateClauseCommand(p.ClauseId, p.ClauseType);
        return Dispatch(cmd, ClauseRoute, "clause_created", "business.agreement.clause.create_failed", ct);
    }

    [HttpPost("activate")]
    public Task<IActionResult> Activate([FromBody] ApiRequest<ClauseIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ActivateClauseCommand(request.Data.ClauseId);
        return Dispatch(cmd, ClauseRoute, "clause_activated", "business.agreement.clause.activate_failed", ct);
    }

    [HttpPost("supersede")]
    public Task<IActionResult> Supersede([FromBody] ApiRequest<ClauseIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SupersedeClauseCommand(request.Data.ClauseId);
        return Dispatch(cmd, ClauseRoute, "clause_superseded", "business.agreement.clause.supersede_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetClause(Guid id, CancellationToken ct) =>
        LoadReadModel<ClauseReadModel>(
            id,
            "projection_business_agreement_clause",
            "clause_read_model",
            "business.agreement.clause.not_found",
            ct);
}

public sealed record CreateClauseRequestModel(Guid ClauseId, string ClauseType);
public sealed record ClauseIdRequestModel(Guid ClauseId);
