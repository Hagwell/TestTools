using Microsoft.AspNetCore.Mvc;
using TestTools.Services;

namespace TestTools.Controllers
{
    [Route("convert")]
    public class ConvertController : Controller
    {
        private readonly DocumentConversionService _docs;
        private readonly ImageConversionService _images;
        private readonly Base64Service _b64;
        private readonly IWebHostEnvironment _env;

        public ConvertController(DocumentConversionService docs, ImageConversionService images, Base64Service b64, IWebHostEnvironment env)
        {
            _docs = docs;
            _images = images;
            _b64 = b64;
            _env = env;
        }

        private string WebRoot => _env.WebRootPath;

        [HttpPost("/convert/document")]
        public async Task<IActionResult> Document(IFormFile file, string targetExt)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "No file uploaded" });

            var uploads = Path.Combine(WebRoot, "uploads");
            Directory.CreateDirectory(uploads);
            var srcExt = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
            var srcName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var srcPath = Path.Combine(uploads, srcName);
            using (var fs = System.IO.File.Create(srcPath))
            {
                await file.CopyToAsync(fs);
            }

            (byte[] bytes, string contentType, string outExt) result;
            using (var s = System.IO.File.OpenRead(srcPath))
            {
                result = await _docs.ConvertAsync(s, srcExt, targetExt);
            }

            var convertedDir = Path.Combine(WebRoot, "converted");
            Directory.CreateDirectory(convertedDir);
            var outFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyyMMddHHmmss}.{result.outExt}";
            var outPath = Path.Combine(convertedDir, outFileName);
            await System.IO.File.WriteAllBytesAsync(outPath, result.bytes);

            var url = $"/converted/{Uri.EscapeDataString(outFileName)}";
            var previewType = GetPreviewType(result.contentType, result.outExt);

            // DOCX preview: extract text for display
            string? previewText = null;
            if (result.outExt == "docx")
            {
                try
                {
                    using var docStream = new MemoryStream(result.bytes);
                    var doc = new Spire.Doc.Document(docStream);
                    previewText = doc.GetText();
                }
                catch { previewText = null; }
            }

            return Json(new { success = true, url, contentType = result.contentType, fileName = outFileName, previewType, previewText });
        }

        [HttpPost("/convert/image")]
        public async Task<IActionResult> Image(IFormFile file, string targetExt)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "No file uploaded" });
            var uploads = Path.Combine(WebRoot, "uploads");
            Directory.CreateDirectory(uploads);
            var srcExt = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
            var srcName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var srcPath = Path.Combine(uploads, srcName);
            using (var fs = System.IO.File.Create(srcPath))
            {
                await file.CopyToAsync(fs);
            }

            (byte[] bytes, string contentType, string outExt) result;
            using (var s = System.IO.File.OpenRead(srcPath))
            {
                result = await _images.ConvertAsync(s, srcExt, targetExt);
            }

            var convertedDir = Path.Combine(WebRoot, "converted");
            Directory.CreateDirectory(convertedDir);
            var outFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.UtcNow:yyyyMMddHHmmss}.{result.outExt}";
            var outPath = Path.Combine(convertedDir, outFileName);
            await System.IO.File.WriteAllBytesAsync(outPath, result.bytes);

            var url = $"/converted/{Uri.EscapeDataString(outFileName)}";
            var previewType = GetPreviewType(result.contentType, result.outExt);

            return Json(new { success = true, url, contentType = result.contentType, fileName = outFileName, previewType });
        }

        [HttpPost("/convert/tobase64")]
        public async Task<IActionResult> ToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "No file uploaded" });
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var contentType = file.ContentType ?? "application/octet-stream";
            var (dataUrl, _) = _b64.ToBase64(bytes, contentType);
            return Json(new { success = true, base64 = dataUrl, contentType, fileName = file.FileName });
        }

        [HttpPost("/convert/frombase64")]
        public async Task<IActionResult> FromBase64([FromForm] string base64, [FromForm] string? fileName)
        {
            if (string.IsNullOrWhiteSpace(base64)) return BadRequest(new { success = false, message = "Base64 is empty" });
            var (bytes, ct) = _b64.FromBase64(base64);
            var ext = GetExtFromContentType(ct) ?? Path.GetExtension(fileName ?? "").Trim('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) ext = "bin";

            var convertedDir = Path.Combine(WebRoot, "converted");
            Directory.CreateDirectory(convertedDir);
            var safeName = string.IsNullOrWhiteSpace(fileName) ? $"decoded_{DateTime.UtcNow:yyyyMMddHHmmss}.{ext}" : Path.GetFileName(fileName);
            var outPath = Path.Combine(convertedDir, safeName);
            await System.IO.File.WriteAllBytesAsync(outPath, bytes);
            var url = $"/converted/{Uri.EscapeDataString(safeName)}";
            var previewType = GetPreviewType(ct ?? "application/octet-stream", ext);
            return Json(new { success = true, url, contentType = ct, fileName = safeName, previewType });
        }

        [HttpPost("/convert/delete")]
        public IActionResult Delete([FromForm] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return BadRequest(new { success = false });
            var safe = Path.GetFileName(fileName);
            var path = Path.Combine(WebRoot, "converted", safe);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            return Json(new { success = true });
        }

        private string GetPreviewType(string? contentType, string outExt)
        {
            contentType = contentType?.ToLowerInvariant() ?? string.Empty;
            outExt = outExt.ToLowerInvariant();
            if (contentType.StartsWith("image/")) return "image";
            if (contentType == "application/pdf" || outExt == "pdf") return "iframe";
            if (contentType.StartsWith("text/") || outExt is "html" or "txt" or "md") return outExt == "html" ? "iframe" : "text";
            return "download"; // fallback
        }

        private string? GetExtFromContentType(string? ct) => ct?.ToLowerInvariant() switch
        {
            "application/pdf" => "pdf",
            "text/html" => "html",
            "text/plain" => "txt",
            "text/markdown" => "md",
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            "image/bmp" => "bmp",
            "image/webp" => "webp",
            _ => null
        };
    }
}