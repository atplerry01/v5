namespace Whycespace.Projections.Business.Agreement.Approval;

public interface IApprovalViewRepository
{
    Task SaveAsync(ApprovalReadModel model, CancellationToken ct = default);
    Task<ApprovalReadModel?> GetAsync(string id, CancellationToken ct = default);
}
