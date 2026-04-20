namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public enum MediaProcessingKind
{
    Transcode,
    Normalize,
    Compression,
    FormatConversion,
    ThumbnailGeneration,
    WaveformGeneration,
    DerivativeGeneration
}
