using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public readonly record struct ImageDimensions
{
    public int Width { get; }
    public int Height { get; }

    public ImageDimensions(int width, int height)
    {
        Guard.Against(width <= 0, "ImageDimensions width must be > 0.");
        Guard.Against(height <= 0, "ImageDimensions height must be > 0.");
        Width = width;
        Height = height;
    }

    public override string ToString() => $"{Width}x{Height}";
}
