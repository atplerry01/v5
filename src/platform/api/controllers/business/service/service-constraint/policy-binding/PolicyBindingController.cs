using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Business;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.PolicyBinding;

[Authorize]
[ApiController]
[Route("api/service-constraint/policy-binding")]
[ApiExplorerSettings(GroupName = "business.service.service-constraint.policy-binding")]
public sealed class PolicyBindingController : BusinessControllerBase
{
    private static readonly DomainRoute PolicyBindingRoute = new("business", "service", "policy-binding");

    public PolicyBindingController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("create")]
    public Task<IActionResult> Create([FromBody] ApiRequest<CreatePolicyBindingRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreatePolicyBindingCommand(p.PolicyBindingId, p.ServiceDefinitionId, p.PolicyRef, p.Scope);
        return Dispatch(cmd, PolicyBindingRoute, "policy_binding_created", "business.service.policy-binding.create_failed", ct);
    }

    [HttpPost("bind")]
    public Task<IActionResult> Bind([FromBody] ApiRequest<PolicyBindingIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new BindPolicyBindingCommand(request.Data.PolicyBindingId, Clock.UtcNow);
        return Dispatch(cmd, PolicyBindingRoute, "policy_binding_bound", "business.service.policy-binding.bind_failed", ct);
    }

    [HttpPost("unbind")]
    public Task<IActionResult> Unbind([FromBody] ApiRequest<PolicyBindingIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new UnbindPolicyBindingCommand(request.Data.PolicyBindingId, Clock.UtcNow);
        return Dispatch(cmd, PolicyBindingRoute, "policy_binding_unbound", "business.service.policy-binding.unbind_failed", ct);
    }

    [HttpPost("archive")]
    public Task<IActionResult> Archive([FromBody] ApiRequest<PolicyBindingIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchivePolicyBindingCommand(request.Data.PolicyBindingId);
        return Dispatch(cmd, PolicyBindingRoute, "policy_binding_archived", "business.service.policy-binding.archive_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetPolicyBinding(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyBindingReadModel>(
            id,
            "projection_business_service_policy_binding",
            "policy_binding_read_model",
            "business.service.policy-binding.not_found",
            ct);
}

public sealed record CreatePolicyBindingRequestModel(
    Guid PolicyBindingId,
    Guid ServiceDefinitionId,
    string PolicyRef,
    int Scope);

public sealed record PolicyBindingIdRequestModel(Guid PolicyBindingId);
