using Whycespace.Platform.Api.Core.Contracts.Economic;
using Whycespace.Platform.Api.Core.Contracts.Governance;
using Whycespace.Platform.Api.Core.Contracts.Graph;
using Whycespace.Platform.Api.Core.Contracts.Presentation;
using Whycespace.Platform.Api.Core.Contracts.Workflow;

namespace Whycespace.Platform.Api.Core.Services.Presentation;

/// <summary>
/// Pure transformation mapper. Zero logic, zero external calls.
/// Maps domain view models → UI-ready presentation models.
/// Deterministic: same input always produces the same output.
/// </summary>
public sealed class UiMapper : IUiMapper
{
    public UiCard MapWorkflow(WorkflowView workflow) => new()
    {
        Id = workflow.WorkflowId.ToString(),
        Title = FormatWorkflowTitle(workflow.WorkflowKey),
        Subtitle = $"Cluster: {workflow.Cluster}",
        Status = workflow.Status,
        Type = "workflow",
        Timestamp = workflow.StartedAt,
        Tags = new Dictionary<string, string>
        {
            ["workflowKey"] = workflow.WorkflowKey,
            ["steps"] = workflow.Steps.Count.ToString()
        }
    };

    public IReadOnlyList<UiCard> MapWorkflows(IReadOnlyList<WorkflowView> workflows) =>
        workflows.Select(MapWorkflow).ToList();

    public IReadOnlyList<UiTimelineItem> MapWorkflowTimeline(WorkflowTimelineView timeline) =>
        timeline.Events.Select(e => new UiTimelineItem
        {
            Id = e.EventId,
            Label = e.EventType,
            Timestamp = e.Timestamp,
            Status = e.EventType.Contains("Completed") ? "COMPLETED"
                   : e.EventType.Contains("Failed") ? "FAILED"
                   : e.EventType.Contains("Started") ? "RUNNING"
                   : "PENDING",
            Description = e.Description,
            Category = "workflow"
        }).ToList();

    public IReadOnlyList<UiListItem> MapLedgerEntries(IReadOnlyList<LedgerEntryView> entries) =>
        entries.Select(e => new UiListItem
        {
            Id = e.EntryId.ToString(),
            Title = $"{e.Type.ToUpperInvariant()} — {e.Amount:N2} {e.Currency}",
            Description = e.Description ?? $"Ledger entry for account {e.AccountId}",
            Status = "COMPLETED",
            Type = "economic",
            Timestamp = e.Timestamp
        }).ToList();

    public UiCard MapWallet(WalletView wallet) => new()
    {
        Id = wallet.WalletId.ToString(),
        Title = $"Wallet — {wallet.Balance:N2} {wallet.Currency}",
        Subtitle = $"Status: {wallet.Status}",
        Status = wallet.Status,
        Type = "economic",
        Timestamp = wallet.LastTransactionAt,
        Tags = new Dictionary<string, string>
        {
            ["currency"] = wallet.Currency,
            ["balance"] = wallet.Balance.ToString("F2")
        }
    };

    public UiCard MapGovernanceDecision(GovernanceDecisionView decision) => new()
    {
        Id = decision.DecisionId.ToString(),
        Title = $"{decision.Type} Decision",
        Subtitle = $"Authority: {decision.Authority} | Cluster: {decision.Cluster}",
        Status = decision.Status,
        Type = "governance",
        Timestamp = decision.CreatedAt,
        Tags = new Dictionary<string, string>
        {
            ["decisionType"] = decision.Type,
            ["authority"] = decision.Authority
        }
    };

    public IReadOnlyList<UiTimelineItem> MapGovernanceTimeline(GovernanceTimelineView timeline) =>
        timeline.Events.Select(e => new UiTimelineItem
        {
            Id = e.EventId,
            Label = e.EventType,
            Timestamp = e.Timestamp,
            Status = e.EventType,
            Description = e.Description,
            Category = "governance"
        }).ToList();

    public UiGraphData MapSpvGraph(SpvGraphView graph) => new()
    {
        Nodes = graph.Nodes.Select(n => new UiGraphNode
        {
            Id = n.SpvId.ToString(),
            Label = n.Name,
            Type = "spv",
            Status = n.Status,
            Group = n.Cluster
        }).ToList(),
        Edges = graph.Edges.Select(e => new UiGraphEdge
        {
            From = e.FromSpvId.ToString(),
            To = e.ToSpvId.ToString(),
            Type = e.RelationshipType,
            Label = e.Label
        }).ToList()
    };

    private static string FormatWorkflowTitle(string workflowKey)
    {
        var parts = workflowKey.Split('.');
        if (parts.Length < 2) return workflowKey;
        return string.Join(" ", parts.Select(p =>
            string.IsNullOrEmpty(p) ? p : char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
