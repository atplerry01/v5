using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Validity;

[Authorize]
[ApiController]
[Route("api/commitment/validity")]
[ApiExplorerSettings(GroupName = "business.agreement.commitment.validity")]
public sealed class ValidityController : BusinessControllerBase
{
    private static readonly DomainRoute ValidityRoute = new("business", "agreement", "validity");

    public ValidityController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreateValidityRequestModel> request, CancellationToken ct)
    {
        var cmd = new CreateValidityCommand(request.Data.ValidityId);
        return Dispatch(cmd, ValidityRoute, "validity_created", "business.agreement.validity.create_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ValidityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireValidityCommand(request.Data.ValidityId);
        return Dispatch(cmd, ValidityRoute, "validity_expired", "business.agreement.validity.expire_failed", ct);
    }

    [HttpPost("invalidate")]
    public Task<IActionResult> Invalidate([FromBody] ApiRequest<ValidityIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new InvalidateValidityCommand(request.Data.ValidityId);
        return Dispatch(cmd, ValidityRoute, "validity_invalidated", "business.agreement.validity.invalidate_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetValidity(Guid id, CancellationToken ct) =>
        LoadReadModel<ValidityReadModel>(
            id,
            "projection_business_agreement_validity",
            "validity_read_model",
            "business.agreement.validity.not_found",
            ct);
}

public sealed record CreateValidityRequestModel(Guid ValidityId);
public sealed record ValidityIdRequestModel(Guid ValidityId);
