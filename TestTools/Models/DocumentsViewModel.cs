using System.ComponentModel.DataAnnotations;

namespace TestTools.Models
{
    public class DocumentModel
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public long Size { get; set; }
        public string SizeFormatted => FormatSize(Size);

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{Math.Round(size, 2)} {sizes[order]}";
        }
    }

    public class DocumentUploadModel
    {
        [Required]
        public IFormFile? File { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class DocumentsViewModel
    {
        public List<DocumentModel> EducationDocuments { get; set; } = new List<DocumentModel>();
        public List<DocumentModel> FrameworkDocuments { get; set; } = new List<DocumentModel>();
        public List<DocumentModel> SupportDocuments { get; set; } = new List<DocumentModel>();
        public List<DocumentModel> InstructionsDocuments { get; set; } = new List<DocumentModel>();
    }
}