namespace Whycespace.Domain.SharedKernel.Primitive.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
