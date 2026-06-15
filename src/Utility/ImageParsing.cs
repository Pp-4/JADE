using System;

using SkiaSharp;

namespace JADE.Utility;

public static class ImageParsing
{
    public static byte[] ConvertWebpToPng(byte[] imageData)
    {

        using var stream = new SKMemoryStream(imageData);

        using var rawData = SKBitmap.Decode(stream);
        if (rawData is not null)
        {
            var image = SKImage.FromBitmap(rawData);
            return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }
        throw new NullReferenceException("Ivalid Image!");
    }
}
