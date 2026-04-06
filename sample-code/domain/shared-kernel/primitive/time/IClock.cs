namespace Whycespace.Domain.SharedKernel.Primitive.Time;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset => new(UtcNow, TimeSpan.Zero);
}
