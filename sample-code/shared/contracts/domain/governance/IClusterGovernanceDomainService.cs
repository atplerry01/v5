namespace Whycespace.Shared.Contracts.Domain.Governance;

public interface IClusterGovernanceDomainService
{
    Task<DomainOperationResult> ProposeAsync(
        DomainExecutionContext context,
        string id,
        string clusterId,
        string decisionType,
        string decisionHash);

    Task<DomainOperationResult> ApproveAsync(
        DomainExecutionContext context,
        string id);

    Task<DomainOperationResult> ExecuteAsync(
        DomainExecutionContext context,
        string id);

    Task<DomainOperationResult> RejectAsync(
        DomainExecutionContext context,
        string id,
        string reason);
}
