using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public readonly record struct FrameRate
{
    public decimal FramesPerSecond { get; }

    public FrameRate(decimal framesPerSecond)
    {
        Guard.Against(framesPerSecond <= 0m, "FrameRate must be > 0.");
        Guard.Against(framesPerSecond > 1000m, "FrameRate cannot exceed 1000 fps.");
        FramesPerSecond = framesPerSecond;
    }

    public override string ToString() => $"{FramesPerSecond} fps";
}
