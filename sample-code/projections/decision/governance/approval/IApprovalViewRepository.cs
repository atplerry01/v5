namespace Whycespace.Projections.Decision.Governance.Approval;

public interface IApprovalViewRepository
{
    Task SaveAsync(ApprovalReadModel model, CancellationToken ct = default);
    Task<ApprovalReadModel?> GetAsync(string id, CancellationToken ct = default);
}
