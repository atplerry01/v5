namespace Whycespace.Shared.Constants;

public static class SystemConstants
{
    public const string SystemName = "Whycespace";
    public const string Version = "4.0.0";

    public static class Limits
    {
        public const int MaxNameLength = 256;
        public const int MaxDescriptionLength = 2048;
        public const int MaxMetadataEntries = 64;
        public const int MaxRetryAttempts = 5;
    }

    public static class Defaults
    {
        public const int PageSize = 25;
        public const int TimeoutSeconds = 30;
    }
}
