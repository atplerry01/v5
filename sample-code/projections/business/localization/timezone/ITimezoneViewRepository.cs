namespace Whycespace.Projections.Business.Localization.Timezone;

public interface ITimezoneViewRepository
{
    Task SaveAsync(TimezoneReadModel model, CancellationToken ct = default);
    Task<TimezoneReadModel?> GetAsync(string id, CancellationToken ct = default);
}
