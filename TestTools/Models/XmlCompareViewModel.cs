namespace TestTools.Models
{
    public class XmlCompareViewModel
    {
        public IFormFile? FirstXmlFile { get; set; }
        public IFormFile? SecondXmlFile { get; set; }
        public string? FirstXmlContent { get; set; }
        public string? SecondXmlContent { get; set; }
        public string? MergedXmlContent { get; set; }
        public List<XmlDifference>? Differences { get; set; }
        public bool ComparisonPerformed { get; set; } = false;
        public bool MergePerformed { get; set; } = false;
    }

    public class XmlDifference
    {
        public string? Path { get; set; }
        public string? FirstValue { get; set; }
        public string? SecondValue { get; set; }
        public DifferenceType Type { get; set; }
    }

    public enum DifferenceType
    {
        ValueDifference,
        AttributeDifference,
        MissingNode,
        ExtraNode
    }
}
