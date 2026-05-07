using System.Collections.Generic;
using System.Linq;

namespace JADE.Utility;


public struct ImageFormat(string Name, byte[] Signature, int ByteOffset)
{
    public string Name = Name;
    public byte[] Signature = Signature;
    public int ByteOffset = ByteOffset;
}
public static class ImageData
{
    static readonly ImageFormat[] ImageType = [
        new ("png",  [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], 0),
        new ("avif", [0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66], 4),
        new ("gif",  [0x47, 0x49, 0x46, 0x38, 0x37, 0x61], 0),//2 gif standards
        new ("gif",  [0x47, 0x49, 0x46, 0x38, 0x39, 0x61], 0),
        new ("webp", [0x57, 0x45, 0x42, 0x50], 8),
        new ("bmp",  [0x42, 0x4D], 0),
        new ("jpg",  [0xFF, 0xD8, 0xFF], 0),
        new ("ico",  [0x00, 0x00, 0x01, 0x00], 0)

    ];
    public static string? GetImageFileType(byte[] bytes)
    {
        string? type = GetImageType(ref bytes);
        if (type is null) return null;
        else return '.' + type;
    }
    static string? GetImageType(ref byte[] bytes)
    {
        foreach (var type in ImageType)
        {
            if (TestFileType(ref bytes, type.Signature, type.ByteOffset))
                return type.Name;
        }
        return null;
    }

    static bool TestFileType(ref byte[] source, byte[] sig, int offset)
    {
        if (source.Length < sig.Length + offset) return false;
        for (int i = 0; i < sig.Length; i++)
        {
            if (source[i + offset] != sig[i])
                return false;
        }
        return true;
    }
}