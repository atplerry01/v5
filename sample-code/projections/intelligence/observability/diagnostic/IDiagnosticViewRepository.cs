namespace Whycespace.Projections.Intelligence.Observability.Diagnostic;

public interface IDiagnosticViewRepository
{
    Task SaveAsync(DiagnosticReadModel model, CancellationToken ct = default);
    Task<DiagnosticReadModel?> GetAsync(string id, CancellationToken ct = default);
}
