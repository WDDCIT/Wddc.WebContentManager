using System;
using SkiaSharp;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer
{
    /// <summary>
    /// Pictures for the flyer, made small enough for a PDF people download: the website's product pictures are
    /// 1200 px squares of up to ~150 KB, and a flyer has 150+ of them. SkiaSharp (MIT) comes with PDFtoImage.
    /// </summary>
    public static class ClearanceFlyerPictures
    {
        /// <summary>A picture that fits in <paramref name="maxSide"/> pixels; null when the bytes are not a picture.</summary>
        public sealed class Picture
        {
            public byte[] Bytes { get; init; }
            public int Width { get; init; }
            public int Height { get; init; }
        }

        /// <summary>
        /// Scales a JPEG or PNG down to fit in maxSide x maxSide (never up). A PNG with transparency stays a PNG
        /// (a cover logo would turn black as a JPEG); anything else becomes a JPEG.
        /// </summary>
        public static Picture Fit(byte[] bytes, int maxSide, int jpegQuality = 82)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            using var original = SKBitmap.Decode(bytes);
            if (original == null || original.Width <= 0 || original.Height <= 0)
            {
                return null;
            }

            var scale = Math.Min(1.0, maxSide / (double)Math.Max(original.Width, original.Height));
            var width = Math.Max(1, (int)Math.Round(original.Width * scale));
            var height = Math.Max(1, (int)Math.Round(original.Height * scale));

            using var resized = scale < 1.0
                ? original.Resize(new SKImageInfo(width, height, original.ColorType, original.AlphaType), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
                : original.Copy();
            if (resized == null)
            {
                return null;
            }

            var keepPng = original.AlphaType != SKAlphaType.Opaque && HasTransparency(resized);
            using var image = SKImage.FromBitmap(resized);
            using var data = keepPng ? image.Encode(SKEncodedImageFormat.Png, 100) : image.Encode(SKEncodedImageFormat.Jpeg, jpegQuality);

            return data == null ? null : new Picture { Bytes = data.ToArray(), Width = width, Height = height };
        }

        // A sampled look for any pixel that is not fully opaque.
        private static bool HasTransparency(SKBitmap bitmap)
        {
            var stepX = Math.Max(1, bitmap.Width / 64);
            var stepY = Math.Max(1, bitmap.Height / 64);
            for (var y = 0; y < bitmap.Height; y += stepY)
            {
                for (var x = 0; x < bitmap.Width; x += stepX)
                {
                    if (bitmap.GetPixel(x, y).Alpha < 255)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
