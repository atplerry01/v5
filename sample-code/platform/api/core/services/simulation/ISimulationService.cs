using Whycespace.Platform.Api.Core.Contracts.Simulation;

namespace Whycespace.Platform.Api.Core.Services.Simulation;

/// <summary>
/// Read-only simulation query service.
/// Delegates to T3I simulation engines via DownstreamAdapter.
///
/// MUST NOT:
/// - Contain simulation logic (pure delegation)
/// - Call domain aggregates or runtime execution
/// - Call T2E engines
/// - Mutate any state, emit events, or write to DB
/// - Trigger workflows
///
/// Flow: Request -> Adapter -> T3I Engine -> Result -> Map -> Response
/// </summary>
public interface ISimulationService
{
    Task<SimulationResultView> SimulateAsync(SimulationRequest request, CancellationToken cancellationToken = default);
}
