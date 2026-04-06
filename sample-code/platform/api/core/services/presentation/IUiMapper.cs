using Whycespace.Platform.Api.Core.Contracts.Economic;
using Whycespace.Platform.Api.Core.Contracts.Governance;
using Whycespace.Platform.Api.Core.Contracts.Graph;
using Whycespace.Platform.Api.Core.Contracts.Presentation;
using Whycespace.Platform.Api.Core.Contracts.Workflow;

namespace Whycespace.Platform.Api.Core.Services.Presentation;

/// <summary>
/// Pure transformation mapper from domain view models to UI-ready presentation models.
///
/// MUST NOT:
/// - Contain business logic
/// - Make external calls
/// - Enrich data beyond input
/// - Compute derived values
///
/// Converts: WorkflowView, LedgerEntryView, GovernanceDecisionView, SpvGraphView → UI models
/// </summary>
public interface IUiMapper
{
    UiCard MapWorkflow(WorkflowView workflow);
    IReadOnlyList<UiCard> MapWorkflows(IReadOnlyList<WorkflowView> workflows);
    IReadOnlyList<UiTimelineItem> MapWorkflowTimeline(WorkflowTimelineView timeline);
    IReadOnlyList<UiListItem> MapLedgerEntries(IReadOnlyList<LedgerEntryView> entries);
    UiCard MapWallet(WalletView wallet);
    UiCard MapGovernanceDecision(GovernanceDecisionView decision);
    IReadOnlyList<UiTimelineItem> MapGovernanceTimeline(GovernanceTimelineView timeline);
    UiGraphData MapSpvGraph(SpvGraphView graph);
}
