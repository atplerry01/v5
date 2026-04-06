namespace Whycespace.Projections.Intelligence.Observability.Alert;

public interface IAlertViewRepository
{
    Task SaveAsync(AlertReadModel model, CancellationToken ct = default);
    Task<AlertReadModel?> GetAsync(string id, CancellationToken ct = default);
}
