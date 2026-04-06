namespace Whycespace.Shared.Contracts.Command;

public interface ICommand
{
    string CommandId { get; }
    DateTime Timestamp { get; }
}
