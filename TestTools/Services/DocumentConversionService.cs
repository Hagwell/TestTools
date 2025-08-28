namespace TestTools.Services
{
    public class DocumentConversionService
    {
        public async Task<(byte[] bytes, string contentType, string outExt)> ConvertAsync(Stream input, string sourceExt, string targetExt)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            sourceExt = NormalizeExt(sourceExt);
            targetExt = NormalizeExt(targetExt);

            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            var data = ms.ToArray();

            try
            {
                if (sourceExt == targetExt)
                {
                    return (data, GetContentType(targetExt), targetExt);
                }

                // PDF to DOCX
                if (sourceExt == "pdf" && targetExt == "docx")
                {
                    return PdfToDocx_Spire(data);
                }

                // DOCX to PDF
                if (sourceExt == "docx" && targetExt == "pdf")
                {
                    return DocxToPdf_Spire(data);
                }

                // PDF to HTML
                if (sourceExt == "pdf" && targetExt == "html")
                {
                    return PdfToHtml_Spire(data);
                }

                // PDF to Markdown
                if (sourceExt == "pdf" && targetExt == "md")
                {
                    return PdfToMarkdown_Spire(data);
                }

                // PDF to Text
                if (sourceExt == "pdf" && targetExt == "txt")
                {
                    return PdfToText_Spire(data);
                }

                // DOCX to HTML
                if (sourceExt == "docx" && targetExt == "html")
                {
                    return DocxToHtml_Spire(data);
                }

                // DOCX to Markdown
                if (sourceExt == "docx" && targetExt == "md")
                {
                    return DocxToMarkdown_Spire(data);
                }

                // DOCX to Text
                if (sourceExt == "docx" && targetExt == "txt")
                {
                    return DocxToText_Spire(data);
                }

                // Fallback for other formats (md, txt, html from txt/md/html)
                if (targetExt == "html" && sourceExt == "md")
                {
                    var mdHtml = Markdig.Markdown.ToHtml(System.Text.Encoding.UTF8.GetString(data));
                    return (System.Text.Encoding.UTF8.GetBytes(mdHtml), "text/html", "html");
                }
                if (targetExt == "html" && sourceExt == "txt")
                {
                    var txt = System.Text.Encoding.UTF8.GetString(data);
                    var escaped = System.Net.WebUtility.HtmlEncode(txt).Replace("\n", "<br/>");
                    var wrapped = $"<html><head><meta charset=\"utf-8\"></head><body><pre>{escaped}</pre></body></html>";
                    return (System.Text.Encoding.UTF8.GetBytes(wrapped), "text/html", "html");
                }
                if (targetExt == "md" && sourceExt == "txt")
                {
                    return (data, "text/markdown", "md");
                }
                if (targetExt == "txt" && sourceExt == "md")
                {
                    var md = System.Text.Encoding.UTF8.GetString(data);
                    var mdText = HtmlToPlainText(Markdig.Markdown.ToHtml(md));
                    return (System.Text.Encoding.UTF8.GetBytes(mdText), "text/plain", "txt");
                }
                if (targetExt == "txt" && sourceExt == "html")
                {
                    var htmlText = HtmlToPlainText(System.Text.Encoding.UTF8.GetString(data));
                    return (System.Text.Encoding.UTF8.GetBytes(htmlText), "text/plain", "txt");
                }
                if (targetExt == "md" && sourceExt == "html")
                {
                    var html = System.Text.Encoding.UTF8.GetString(data);
                    var text = HtmlToPlainText(html);
                    return (System.Text.Encoding.UTF8.GetBytes(text), "text/markdown", "md");
                }
                if (targetExt == "html" && sourceExt == "html")
                {
                    return (data, "text/html", "html");
                }
                if (targetExt == "txt" && sourceExt == "txt")
                {
                    return (data, "text/plain", "txt");
                }
                if (targetExt == "md" && sourceExt == "md")
                {
                    return (data, "text/markdown", "md");
                }

                throw new NotSupportedException($"Unsupported target format: {targetExt}");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Conversion failed: {ex.Message}";
                var errorBytes = System.Text.Encoding.UTF8.GetBytes(errorMsg);
                return (errorBytes, "text/plain", "txt");
            }
        }

        // PDF to DOCX using FreeSpire.PDF
        private (byte[] bytes, string contentType, string outExt) PdfToDocx_Spire(byte[] data)
        {
            try
            {
                using var pdfStream = new MemoryStream(data);
                var pdf = new Spire.Pdf.PdfDocument();
                pdf.LoadFromStream(pdfStream);

                var doc = new Spire.Doc.Document();
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    var text = pdf.Pages[i].ExtractText();
                    var section = doc.AddSection();
                    var para = section.AddParagraph();
                    para.AppendText(text);
                }

                using var outStream = new MemoryStream();
                doc.SaveToStream(outStream, Spire.Doc.FileFormat.Docx);
                return (outStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx");
            }
            catch (Exception ex)
            {
                var errorMsg = $"PDF to DOCX conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // DOCX to PDF using FreeSpire.Doc
        private (byte[] bytes, string contentType, string outExt) DocxToPdf_Spire(byte[] data)
        {
            try
            {
                using var docStream = new MemoryStream(data);
                var doc = new Spire.Doc.Document(docStream);
                using var outStream = new MemoryStream();
                doc.SaveToStream(outStream, Spire.Doc.FileFormat.PDF);
                return (outStream.ToArray(), "application/pdf", "pdf");
            }
            catch (Exception ex)
            {
                var errorMsg = $"DOCX to PDF conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // PDF to HTML using FreeSpire.PDF
        private (byte[] bytes, string contentType, string outExt) PdfToHtml_Spire(byte[] data)
        {
            try
            {
                using var pdfStream = new MemoryStream(data);
                var pdf = new Spire.Pdf.PdfDocument();
                pdf.LoadFromStream(pdfStream);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("<html><body>");
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    var text = pdf.Pages[i].ExtractText();
                    sb.AppendLine($"<div><pre>{System.Net.WebUtility.HtmlEncode(text)}</pre></div>");
                }
                sb.AppendLine("</body></html>");
                var htmlBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return (htmlBytes, "text/html", "html");
            }
            catch (Exception ex)
            {
                var errorMsg = $"PDF to HTML conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // PDF to Markdown
        private (byte[] bytes, string contentType, string outExt) PdfToMarkdown_Spire(byte[] data)
        {
            try
            {
                using var pdfStream = new MemoryStream(data);
                var pdf = new Spire.Pdf.PdfDocument();
                pdf.LoadFromStream(pdfStream);

                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    var text = pdf.Pages[i].ExtractText();
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
                var mdBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return (mdBytes, "text/markdown", "md");
            }
            catch (Exception ex)
            {
                var errorMsg = $"PDF to Markdown conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // PDF to Text
        private (byte[] bytes, string contentType, string outExt) PdfToText_Spire(byte[] data)
        {
            try
            {
                using var pdfStream = new MemoryStream(data);
                var pdf = new Spire.Pdf.PdfDocument();
                pdf.LoadFromStream(pdfStream);

                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    var text = pdf.Pages[i].ExtractText();
                    sb.AppendLine(text);
                }
                var txtBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return (txtBytes, "text/plain", "txt");
            }
            catch (Exception ex)
            {
                var errorMsg = $"PDF to Text conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // DOCX to HTML using FreeSpire.Doc
        private (byte[] bytes, string contentType, string outExt) DocxToHtml_Spire(byte[] data)
        {
            try
            {
                using var docStream = new MemoryStream(data);
                var doc = new Spire.Doc.Document(docStream);
                using var outStream = new MemoryStream();
                doc.SaveToStream(outStream, Spire.Doc.FileFormat.Html);
                return (outStream.ToArray(), "text/html", "html");
            }
            catch (Exception ex)
            {
                var errorMsg = $"DOCX to HTML conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // DOCX to Markdown (simple text extraction)
        private (byte[] bytes, string contentType, string outExt) DocxToMarkdown_Spire(byte[] data)
        {
            try
            {
                using var docStream = new MemoryStream(data);
                var doc = new Spire.Doc.Document(docStream);
                var text = doc.GetText();
                var mdBytes = System.Text.Encoding.UTF8.GetBytes(text);
                return (mdBytes, "text/markdown", "md");
            }
            catch (Exception ex)
            {
                var errorMsg = $"DOCX to Markdown conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        // DOCX to Text
        private (byte[] bytes, string contentType, string outExt) DocxToText_Spire(byte[] data)
        {
            try
            {
                using var docStream = new MemoryStream(data);
                var doc = new Spire.Doc.Document(docStream);
                var text = doc.GetText();
                var txtBytes = System.Text.Encoding.UTF8.GetBytes(text);
                return (txtBytes, "text/plain", "txt");
            }
            catch (Exception ex)
            {
                var errorMsg = $"DOCX to Text conversion failed: {ex.Message}";
                return (System.Text.Encoding.UTF8.GetBytes(errorMsg), "text/plain", "txt");
            }
        }

        private string HtmlToPlainText(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return HtmlAgilityPack.HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
        }

        private string NormalizeExt(string ext)
        {
            ext = ext.Trim().Trim('.').ToLowerInvariant();
            return ext;
        }

        private string GetContentType(string ext)
        {
            return ext switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "html" => "text/html",
                "txt" => "text/plain",
                "md" => "text/markdown",
                _ => "application/octet-stream"
            };
        }
    }
}
