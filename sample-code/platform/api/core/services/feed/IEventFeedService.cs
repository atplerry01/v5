using Whycespace.Platform.Api.Core.Contracts.Feed;

namespace Whycespace.Platform.Api.Core.Services.Feed;

/// <summary>
/// Read-only event feed query service.
/// All data sourced from CQRS projections (read models) via ProjectionAdapter.
///
/// MUST NOT:
/// - Subscribe to Kafka or any message bus
/// - Replay events from event stores
/// - Compute or aggregate events
/// - Modify any state (except notification read-status)
///
/// Platform surfaces projection data — nothing more.
/// </summary>
public interface IEventFeedService
{
    Task<IReadOnlyList<EventFeedItemView>> GetFeedAsync(
        Guid identityId, int page, int pageSize, CancellationToken ct);

    Task<IReadOnlyList<NotificationView>> GetNotificationsAsync(
        Guid identityId, CancellationToken ct);

    Task<int> GetUnreadCountAsync(
        Guid identityId, CancellationToken ct);

    Task MarkAsReadAsync(
        Guid identityId, string notificationId, CancellationToken ct);
}
