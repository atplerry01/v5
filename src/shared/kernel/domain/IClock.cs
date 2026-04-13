namespace Whycespace.Shared.Kernel.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
