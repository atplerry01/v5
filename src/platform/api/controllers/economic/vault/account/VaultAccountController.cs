using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Economic.Vault.Account;

[Authorize]
[ApiController]
[Route("api/economic/vault/account")]
[ApiExplorerSettings(GroupName = "economic.vault.account")]
public sealed class VaultAccountController : ControllerBase
{
    private static readonly DomainRoute VaultAccountRoute =
        new("economic", "vault", "account");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly string _projectionsConnectionString;

    public VaultAccountController(
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

    [HttpPost("create")]
    public Task<IActionResult> CreateVaultAccount(
        [FromBody] ApiRequest<CreateVaultAccountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var vaultAccountId = _idGenerator.Generate(
            $"economic:vault:account:{p.OwnerSubjectId}:{p.Currency}");

        var cmd = new CreateVaultAccountCommand(vaultAccountId, p.OwnerSubjectId, p.Currency);
        return Dispatch(cmd, "vault_account_created", "economic.vault.account.create_failed", ct);
    }

    [HttpPost("fund")]
    public Task<IActionResult> FundVault(
        [FromBody] ApiRequest<AmountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new FundVaultCommand(p.VaultAccountId, p.Amount, p.Currency);
        return Dispatch(cmd, "vault_funded", "economic.vault.account.fund_failed", ct);
    }

    [HttpPost("invest")]
    public Task<IActionResult> InvestVault(
        [FromBody] ApiRequest<AmountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new InvestCommand(p.VaultAccountId, p.Amount, p.Currency);
        return Dispatch(cmd, "vault_invested", "economic.vault.account.invest_failed", ct);
    }

    [HttpPost("apply-revenue")]
    public Task<IActionResult> ApplyRevenue(
        [FromBody] ApiRequest<AmountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new ApplyRevenueCommand(p.VaultAccountId, p.Amount, p.Currency);
        return Dispatch(cmd, "vault_revenue_applied", "economic.vault.account.apply_revenue_failed", ct);
    }

    [HttpPost("debit")]
    public Task<IActionResult> DebitSlice(
        [FromBody] ApiRequest<SliceAmountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new DebitSliceCommand(p.VaultAccountId, p.Slice, p.Amount);
        return Dispatch(cmd, "vault_debited", "economic.vault.account.debit_failed", ct);
    }

    [HttpPost("credit")]
    public Task<IActionResult> CreditSlice(
        [FromBody] ApiRequest<SliceAmountRequestModel> request,
        CancellationToken ct)
    {
        var p = request.Data;
        var cmd = new CreditSliceCommand(p.VaultAccountId, p.Slice, p.Amount);
        return Dispatch(cmd, "vault_credited", "economic.vault.account.credit_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVaultAccount(Guid id, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_projectionsConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(
            "SELECT state FROM projection_economic_vault_account.vault_account_read_model " +
            "WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return NotFound(ApiResponse.Fail(
                "economic.vault.account.not_found",
                $"VaultAccount {id} not found.",
                _clock.UtcNow));

        var stateJson = reader.GetString(0);
        // D10/closed-loop: projection writer emits camelCase JSON; default
        // STJ deserialize is case-sensitive and silently produces a default
        // record. JsonSerializerDefaults.Web turns on PropertyNameCaseInsensitive
        // so the read API renders the projection's actual state.
        var model = JsonSerializer.Deserialize<VaultAccountReadModel>(stateJson,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException(
                $"Failed to deserialize VaultAccountReadModel for aggregate {id}.");

        return Ok(ApiResponse.Ok(model, this.RequestCorrelationId(), _clock.UtcNow));
    }

    private async Task<IActionResult> Dispatch(
        object command,
        string ack,
        string failureCode,
        CancellationToken ct)
    {
        var result = await _dispatcher.DispatchAsync(command, VaultAccountRoute, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok(new CommandAck(ack), result.CorrelationId, _clock.UtcNow))
            : BadRequest(ApiResponse.Fail(failureCode, result.Error ?? "Unknown error", _clock.UtcNow));
    }
}

public sealed record CreateVaultAccountRequestModel(
    Guid OwnerSubjectId,
    string Currency);

public sealed record AmountRequestModel(
    Guid VaultAccountId,
    decimal Amount,
    string Currency);

public sealed record SliceAmountRequestModel(
    Guid VaultAccountId,
    VaultSliceType Slice,
    decimal Amount);
