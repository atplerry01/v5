namespace Whycespace.Projections.Business.Notification.Channel;

public interface IChannelViewRepository
{
    Task SaveAsync(ChannelReadModel model, CancellationToken ct = default);
    Task<ChannelReadModel?> GetAsync(string id, CancellationToken ct = default);
}
