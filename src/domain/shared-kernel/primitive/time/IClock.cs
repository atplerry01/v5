namespace Whycespace.Domain.SharedKernel.Primitive.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
