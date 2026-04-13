namespace PlaywrightSharp.Utility;

public static class ImageData
{
    static readonly (string, byte[])[] ImageType = [
        ("png",  [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
        ("webp", [0x57, 0x45, 0x42, 0x50]),
        ("gif",  [0x47, 0x49, 0x46]),
        ("bmp",  [0x42, 0x4D]),
        ("jpg",  [0xFF, 0xD8])
    ];
    public static string GetImageType(byte[] bytes)
    {
        foreach (var type in ImageType)
        {
            int i = 0;
            while (bytes[i++] != type.Item2[0] && i < 1024) ;
            if (i >= bytes.Length) break;
            int j = 0;
            while (j < type.Item2.Length)
            {
                if (bytes[i + j] == type.Item2[j]) j++;
                else break;
            }
            return type.Item1;
        }
        return string.Empty;
    }
    public static string GetImageFileType(byte[] bytes)
    {
        string type = GetImageType(bytes);
        if (type != string.Empty) return '.' + type;
        return string.Empty;
    }
}