namespace Whyce.Shared.Kernel.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
