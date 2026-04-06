namespace Whycespace.Projections.Intelligence.Geo.Geofence;

public interface IGeofenceViewRepository
{
    Task SaveAsync(GeofenceReadModel model, CancellationToken ct = default);
    Task<GeofenceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
