namespace Whycespace.Projections.Core.Financialcontrol.ApprovalControl;

public interface IApprovalControlViewRepository
{
    Task SaveAsync(ApprovalControlReadModel model, CancellationToken ct = default);
    Task<ApprovalControlReadModel?> GetAsync(string id, CancellationToken ct = default);
}
