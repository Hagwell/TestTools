using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace TestTools.Services
{
    public class ImageConversionService
    {
        private static readonly HashSet<string> Supported = new(new[] { "png", "jpg", "jpeg", "gif", "bmp", "webp" });

        public async Task<(byte[] bytes, string contentType, string outExt)> ConvertAsync(Stream input, string sourceExt, string targetExt)
        {
            sourceExt = NormalizeExt(sourceExt);
            targetExt = NormalizeExt(targetExt);

            if (!Supported.Contains(targetExt))
                throw new NotSupportedException($"Unsupported target image format: {targetExt}");

            using var image = await Image.LoadAsync(input);
            using var ms = new MemoryStream();
            var encoder = GetEncoder(targetExt);
            await image.SaveAsync(ms, encoder);
            var bytes = ms.ToArray();
            return (bytes, GetContentType(targetExt), targetExt);
        }

        private IImageEncoder GetEncoder(string ext) => ext switch
        {
            "png" => new PngEncoder(),
            "jpg" or "jpeg" => new JpegEncoder { Quality = 90 },
            "gif" => new GifEncoder(),
            "bmp" => new BmpEncoder(),
            "webp" => new WebpEncoder(),
            _ => throw new NotSupportedException($"Unsupported image format: {ext}")
        };

        private string GetContentType(string ext) => ext switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };

        private static string NormalizeExt(string ext) => ext.Trim().Trim('.').ToLowerInvariant();
    }
}
