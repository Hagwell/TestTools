namespace TestTools.Models
{
    public class ConversionModel
    {
        public IFormFile? File { get; set; }
        public string? OutputFormat { get; set; }
        public bool UseIntermediateFormat { get; set; } = false;
        public string ConversionQuality { get; set; } = "standard";
        public bool PreserveImages { get; set; } = true;
    }

    public class MultipleConversionModel
    {
        public List<IFormFile>? Files { get; set; }
        public string? OutputFormat { get; set; }
        public bool UseIntermediateFormat { get; set; } = false;
        public string ConversionQuality { get; set; } = "standard";
        public bool PreserveImages { get; set; } = true;
    }
}