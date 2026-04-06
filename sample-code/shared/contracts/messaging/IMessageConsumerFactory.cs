namespace Whycespace.Shared.Contracts.Messaging;

/// <summary>
/// Factory for creating message consumers with specific group IDs.
/// Infrastructure adapter implements this to produce transport-bound consumers.
/// </summary>
public interface IMessageConsumerFactory
{
    IMessageConsumer Create(string groupId);
}
