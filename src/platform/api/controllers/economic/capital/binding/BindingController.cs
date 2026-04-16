using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Economic.Capital.Shared;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Capital.Binding;

[Authorize]
[ApiController]
[Route("api/capital/binding")]
[ApiExplorerSettings(GroupName = "economic.capital.binding")]
public sealed class BindingController : CapitalControllerBase
{
    private static readonly DomainRoute BindingRoute = new("economic", "capital", "binding");

    public BindingController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("bind")]
    public Task<IActionResult> Bind([FromBody] ApiRequest<BindRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var bindingId = IdGenerator.Generate($"economic:capital:binding:{p.AccountId}:{p.OwnerId}");
        var cmd = new BindCapitalCommand(bindingId, p.AccountId, p.OwnerId, p.OwnershipType, Clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_bound", "economic.capital.binding.bind_failed", ct);
    }

    [HttpPost("transfer")]
    public Task<IActionResult> TransferBinding([FromBody] ApiRequest<TransferBindingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new TransferBindingOwnershipCommand(p.BindingId, p.NewOwnerId, p.NewOwnershipType, Clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_binding_transferred", "economic.capital.binding.transfer_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> ReleaseBinding([FromBody] ApiRequest<ReleaseBindingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ReleaseBindingCommand(p.BindingId, Clock.UtcNow);
        return Dispatch(cmd, BindingRoute, "capital_binding_released", "economic.capital.binding.release_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetBinding(Guid id, CancellationToken ct) =>
        LoadReadModel<CapitalBindingReadModel>(
            id,
            "projection_economic_capital_binding",
            "capital_binding_read_model",
            "economic.capital.binding.not_found",
            ct);
}

public sealed record BindRequestModel(Guid AccountId, Guid OwnerId, int OwnershipType);
public sealed record TransferBindingRequestModel(Guid BindingId, Guid NewOwnerId, int NewOwnershipType);
public sealed record ReleaseBindingRequestModel(Guid BindingId);
