namespace Whycespace.Projections.Business.Notification.Preference;

public interface IPreferenceViewRepository
{
    Task SaveAsync(PreferenceReadModel model, CancellationToken ct = default);
    Task<PreferenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
