namespace TestTools.Models
{
    public class WebElement
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string RelativeXPath { get; set; }
        public string FullXPath { get; set; }
        public bool IsSelected { get; set; }
        public string ElementType { get; set; }
    }

    public class ScrapeRequest
    {
        public List<string> Urls { get; set; } = new List<string>();
    }

    public class ScrapeResult
    {
        public List<WebElement> Elements { get; set; } = new List<WebElement>();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
