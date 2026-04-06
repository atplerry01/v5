using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Intelligence.Identity;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// T3I Identity Graph Engine — builds and traverses the identity intelligence graph.
///
/// Builds a graph from events: identities, devices, sessions, services.
/// Manages nodes + edges. Supports traversal.
///
/// Stateless. No persistence. Graph is built from projection data per invocation.
/// Uses IIdentityIntelligenceDomainService instead of direct domain imports.
/// </summary>
public sealed class IdentityGraphEngine
{
    private readonly IClock _clock;
    private readonly IIdentityIntelligenceDomainService _domainService;

    public IdentityGraphEngine(IClock clock, IIdentityIntelligenceDomainService domainService)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    public async Task<GraphResult> BuildAsync(BuildGraphCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = DeterministicIdHelper.FromSeed($"{command.IdentityId}:BuildGraph").ToString("N"),
            ActorId = "system",
            Action = "BuildGraph",
            Domain = "intelligence.identity",
            Timestamp = _clock.UtcNowOffset
        };

        var graphId = DeterministicIdHelper.FromSeed($"{command.IdentityId}:BuildGraph:graphId");
        await _domainService.CreateIdentityGraphAsync(execCtx, graphId);

        // Add nodes via domain service
        foreach (var node in command.Nodes)
        {
            await _domainService.AddIdentityNodeAsync(execCtx, graphId, node.NodeId, node.NodeType);
        }

        var resultNodes = command.Nodes;
        var resultEdges = command.Edges;

        return new GraphResult(
            command.IdentityId,
            resultNodes.Count,
            resultEdges.Count,
            resultNodes,
            resultEdges,
            _clock.UtcNowOffset);
    }
}
