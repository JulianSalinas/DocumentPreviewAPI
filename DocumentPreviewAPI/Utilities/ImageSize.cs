namespace DocumentPreviewAPI.Utilities;

public enum ImageSize
{
    ExtraSmall,
    Small,
    Medium
}

public static class ImageSizeExtensions
{
    public static readonly Dictionary<ImageSize, (int Width, int Height, string Prefix)> SizeMap = new()
    {
        { ImageSize.ExtraSmall, (320, 200, "xs") },
        { ImageSize.Small, (640, 400, "sm") },
        { ImageSize.Medium, (800, 600, "md") }
    };

    public static int GetWidth(this ImageSize imageSize)
    {
        return SizeMap[imageSize].Width;
    }

    public static int GetHeight(this ImageSize imageSize)
    {
        return SizeMap[imageSize].Height;
    }

    public static string GetPrefix(this ImageSize imageSize)
    {
        return SizeMap[imageSize].Prefix;
    }
}
