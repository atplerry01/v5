using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct Dimensions
{
    public int Width { get; }
    public int Height { get; }

    public Dimensions(int width, int height)
    {
        Guard.Against(width <= 0, "Dimensions width must be > 0.");
        Guard.Against(height <= 0, "Dimensions height must be > 0.");
        Width = width;
        Height = height;
    }

    public override string ToString() => $"{Width}x{Height}";
}
