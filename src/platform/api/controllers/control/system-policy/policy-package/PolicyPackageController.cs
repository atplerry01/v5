using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Control;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Control.SystemPolicy.PolicyPackage;

[Authorize]
[ApiController]
[Route("api/control/policy-package")]
[ApiExplorerSettings(GroupName = "control.system-policy.policy-package")]
public sealed class PolicyPackageController : ControlControllerBase
{
    private static readonly DomainRoute Route = new("control", "system-policy", "policy-package");

    public PolicyPackageController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("assemble")]
    public Task<IActionResult> Assemble([FromBody] ApiRequest<AssemblePolicyPackageRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new AssemblePolicyPackageCommand(p.PackageId, p.Name, p.VersionMajor, p.VersionMinor, p.PolicyDefinitionIds);
        return Dispatch(cmd, Route, "policy_package_assembled", "control.system-policy.policy-package.assemble_failed", ct);
    }

    [HttpPost("deploy")]
    public Task<IActionResult> Deploy([FromBody] ApiRequest<PackageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new DeployPolicyPackageCommand(request.Data.PackageId);
        return Dispatch(cmd, Route, "policy_package_deployed", "control.system-policy.policy-package.deploy_failed", ct);
    }

    [HttpPost("retire")]
    public Task<IActionResult> Retire([FromBody] ApiRequest<PackageIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new RetirePolicyPackageCommand(request.Data.PackageId);
        return Dispatch(cmd, Route, "policy_package_retired", "control.system-policy.policy-package.retire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        LoadReadModel<PolicyPackageReadModel>(
            id,
            "projection_control_system_policy_policy_package",
            "policy_package_read_model",
            "control.system-policy.policy-package.not_found",
            ct);
}

public sealed record AssemblePolicyPackageRequestModel(
    Guid PackageId,
    string Name,
    int VersionMajor,
    int VersionMinor,
    IReadOnlyList<string> PolicyDefinitionIds);

public sealed record PackageIdRequestModel(Guid PackageId);
