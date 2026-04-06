using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Intelligence.Identity;

public interface IIdentityIntelligenceDomainService
{
    Task<DomainOperationResult> CreateTrustProfileAsync(DomainExecutionContext context, Guid identityId, IReadOnlyList<object> signals, IReadOnlyList<object> violations);
    Task<DomainOperationResult> CreateBehaviorProfileAsync(DomainExecutionContext context, Guid identityId);
    Task<DomainOperationResult> RecordBehaviorSignalAsync(DomainExecutionContext context, Guid identityId, object signal);
    Task<DomainOperationResult> CreateIdentityGraphAsync(DomainExecutionContext context, Guid graphId);
    Task<DomainOperationResult> AddIdentityNodeAsync(DomainExecutionContext context, Guid graphId, string nodeId, string nodeType);
}
