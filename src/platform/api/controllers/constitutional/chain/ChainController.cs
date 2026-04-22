using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Platform.Api.Controllers.Business;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Constitutional.Chain;

[Authorize]
[ApiController]
[Route("api/constitutional/chain")]
[ApiExplorerSettings(GroupName = "constitutional.chain")]
public sealed class ChainController : BusinessControllerBase
{
    private static readonly DomainRoute AnchorRecordRoute = new("constitutional", "chain", "anchor-record");
    private static readonly DomainRoute EvidenceRecordRoute = new("constitutional", "chain", "evidence-record");
    private static readonly DomainRoute LedgerRoute = new("constitutional", "chain", "ledger");

    public ChainController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    // ── Anchor Record ────────────────────────────────────────────

    [HttpPost("anchor-record/record")]
    public Task<IActionResult> RecordAnchor([FromBody] ApiRequest<RecordAnchorRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = IdGenerator.Generate($"constitutional:chain:anchor-record:{p.CorrelationId}:{p.BlockHash}");
        var cmd = new RecordAnchorCommand(id, p.CorrelationId, p.BlockHash, p.EventHash, p.PreviousBlockHash, p.DecisionHash, p.Sequence, Clock.UtcNow);
        return Dispatch(cmd, AnchorRecordRoute, "anchor_record_created", "constitutional.chain.anchor_record.record_failed", ct);
    }

    [HttpPost("anchor-record/seal")]
    public Task<IActionResult> SealAnchor([FromBody] ApiRequest<AnchorRecordIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SealAnchorCommand(request.Data.AnchorRecordId, Clock.UtcNow);
        return Dispatch(cmd, AnchorRecordRoute, "anchor_record_sealed", "constitutional.chain.anchor_record.seal_failed", ct);
    }

    [HttpGet("anchor-record/{id:guid}")]
    public Task<IActionResult> GetAnchorRecord(Guid id, CancellationToken ct) =>
        LoadReadModel<AnchorRecordReadModel>(
            id,
            "projection_constitutional_chain_anchor_record",
            "anchor_record_read_model",
            "constitutional.chain.anchor_record.not_found",
            ct);

    // ── Evidence Record ──────────────────────────────────────────

    [HttpPost("evidence-record/record")]
    public Task<IActionResult> RecordEvidence([FromBody] ApiRequest<RecordEvidenceRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = IdGenerator.Generate($"constitutional:chain:evidence-record:{p.CorrelationId}:{p.AnchorRecordId}:{p.ActorId}");
        var cmd = new RecordEvidenceCommand(id, p.CorrelationId, p.AnchorRecordId, p.EvidenceType, p.ActorId, p.SubjectId, p.PolicyHash, Clock.UtcNow);
        return Dispatch(cmd, EvidenceRecordRoute, "evidence_record_created", "constitutional.chain.evidence_record.record_failed", ct);
    }

    [HttpPost("evidence-record/archive")]
    public Task<IActionResult> ArchiveEvidence([FromBody] ApiRequest<EvidenceRecordIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ArchiveEvidenceCommand(request.Data.EvidenceRecordId, Clock.UtcNow);
        return Dispatch(cmd, EvidenceRecordRoute, "evidence_record_archived", "constitutional.chain.evidence_record.archive_failed", ct);
    }

    [HttpGet("evidence-record/{id:guid}")]
    public Task<IActionResult> GetEvidenceRecord(Guid id, CancellationToken ct) =>
        LoadReadModel<EvidenceRecordReadModel>(
            id,
            "projection_constitutional_chain_evidence_record",
            "evidence_record_read_model",
            "constitutional.chain.evidence_record.not_found",
            ct);

    // ── Ledger ───────────────────────────────────────────────────

    [HttpPost("ledger/open")]
    public Task<IActionResult> OpenLedger([FromBody] ApiRequest<OpenLedgerRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        var id = IdGenerator.Generate($"constitutional:chain:ledger:{p.LedgerName}");
        var cmd = new OpenLedgerCommand(id, p.LedgerName, Clock.UtcNow);
        return Dispatch(cmd, LedgerRoute, "ledger_opened", "constitutional.chain.ledger.open_failed", ct);
    }

    [HttpPost("ledger/seal")]
    public Task<IActionResult> SealLedger([FromBody] ApiRequest<LedgerIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new SealLedgerCommand(request.Data.LedgerId, Clock.UtcNow);
        return Dispatch(cmd, LedgerRoute, "ledger_sealed", "constitutional.chain.ledger.seal_failed", ct);
    }

    [HttpGet("ledger/{id:guid}")]
    public Task<IActionResult> GetLedger(Guid id, CancellationToken ct) =>
        LoadReadModel<LedgerReadModel>(
            id,
            "projection_constitutional_chain_ledger",
            "ledger_read_model",
            "constitutional.chain.ledger.not_found",
            ct);
}

public sealed record RecordAnchorRequestModel(
    Guid CorrelationId,
    string BlockHash,
    string EventHash,
    string PreviousBlockHash,
    string DecisionHash,
    long Sequence);

public sealed record AnchorRecordIdRequestModel(Guid AnchorRecordId);

public sealed record RecordEvidenceRequestModel(
    Guid CorrelationId,
    Guid AnchorRecordId,
    string EvidenceType,
    string ActorId,
    string SubjectId,
    string PolicyHash);

public sealed record EvidenceRecordIdRequestModel(Guid EvidenceRecordId);

public sealed record OpenLedgerRequestModel(string LedgerName);

public sealed record LedgerIdRequestModel(Guid LedgerId);
