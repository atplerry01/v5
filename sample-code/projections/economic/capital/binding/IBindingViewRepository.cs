namespace Whycespace.Projections.Economic.Capital.Binding;

public interface IBindingViewRepository
{
    Task SaveAsync(BindingReadModel model, CancellationToken ct = default);
    Task<BindingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
