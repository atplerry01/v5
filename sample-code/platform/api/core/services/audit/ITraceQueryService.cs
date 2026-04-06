using Whycespace.Platform.Api.Core.Contracts.Audit;

namespace Whycespace.Platform.Api.Core.Services.Audit;

/// <summary>
/// Read-only trace query service.
/// All data sourced from pre-built trace projections via ProjectionAdapter.
/// CorrelationId is the primary lookup key for end-to-end tracing.
///
/// MUST NOT:
/// - Call engines or domain services
/// - Query event store directly
/// - Reconstruct traces from events
/// - Expose raw event payloads or internal commands
/// - Modify any state
/// </summary>
public interface ITraceQueryService
{
    Task<TraceView?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<TraceView?> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TraceView>> GetByIdentityAsync(Guid identityId, CancellationToken cancellationToken = default);
}
