namespace Whycespace.Shared.Contracts.Domain.Structural;

public interface IClusterDomainService
{
    Task<DomainOperationResult> CreateClusterAsync(DomainExecutionContext context, string id, string name, string jurisdiction);
    Task<DomainOperationResult> ActivateClusterAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> AddClusterAuthorityAsync(DomainExecutionContext context, string clusterId, string authorityId);

    Task<DomainOperationResult> CreateAuthorityAsync(DomainExecutionContext context, string id, string clusterId, string name);
    Task<DomainOperationResult> AddAuthoritySubClusterAsync(DomainExecutionContext context, string authorityId, string subClusterId);

    Task<DomainOperationResult> CreateTopologyAsync(DomainExecutionContext context, string id, string authorityId, string name);
    Task<DomainOperationResult> AddTopologySpvAsync(DomainExecutionContext context, string subClusterId, string spvId);

    Task<DomainOperationResult> CreateSpvAsync(DomainExecutionContext context, string id, string subClusterId, string name);
    Task<DomainOperationResult> ActivateSpvAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> SuspendSpvAsync(DomainExecutionContext context, string id, string reason);
    Task<DomainOperationResult> ReactivateSpvAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> TerminateSpvAsync(DomainExecutionContext context, string id, string reason);
    Task<DomainOperationResult> CloseSpvAsync(DomainExecutionContext context, string id, string auditRecordId);
    Task<DomainOperationResult> AddSpvOperatorAsync(DomainExecutionContext context, string spvId, string operatorId);
    Task<DomainOperationResult> ReplaceSpvOperatorAsync(DomainExecutionContext context, string spvId, string oldOperatorId, string newOperatorId);

    Task<DomainOperationResult> CreateSubClusterAsync(DomainExecutionContext context, string id, string authorityId, string name);
    Task<DomainOperationResult> ActivateSubClusterAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> DeactivateSubClusterAsync(DomainExecutionContext context, string id, string reason);
    Task<DomainOperationResult> AddSubClusterSpvAsync(DomainExecutionContext context, string subClusterId, string spvId);
    Task<DomainOperationResult> RemoveSubClusterSpvAsync(DomainExecutionContext context, string subClusterId, string spvId);

    Task<DomainOperationResult> CreateClassificationAsync(DomainExecutionContext context, string id);
}
