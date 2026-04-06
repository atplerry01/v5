namespace Whycespace.Projections.Intelligence.Knowledge.Ontology;

public interface IOntologyViewRepository
{
    Task SaveAsync(OntologyReadModel model, CancellationToken ct = default);
    Task<OntologyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
