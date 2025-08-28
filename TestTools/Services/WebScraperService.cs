using HtmlAgilityPack;
using TestTools.Models;

namespace TestTools.Services
{
    public interface IWebScraperService
    {
        Task<ScrapeResult> ScrapeUrlsAsync(List<string> urls);
    }

    public class WebScraperService : IWebScraperService
    {
        private readonly HttpClient _httpClient;

        public WebScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ScrapeResult> ScrapeUrlsAsync(List<string> urls)
        {
            var result = new ScrapeResult();

            try
            {
                foreach (var url in urls)
                {
                    try
                    {
                        // Get the HTML content
                        var response = await _httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        var html = await response.Content.ReadAsStringAsync();

                        // Load the HTML document
                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        // Extract elements
                        result.Elements.AddRange(ExtractElements(doc, url));
                    }
                    catch (Exception ex)
                    {
                        // Continue with other URLs if one fails
                        result.ErrorMessage += $"Error scraping {url}: {ex.Message}\n";
                    }
                }

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error during scraping: {ex.Message}";
                return result;
            }
        }

        private List<WebElement> ExtractElements(HtmlDocument doc, string url)
        {
            var elements = new List<WebElement>();

            // Get all input elements
            ProcessElementsByTag(doc, "input", "Input", elements);

            // Get all button elements
            ProcessElementsByTag(doc, "button", "Button", elements);

            // Get all select elements
            ProcessElementsByTag(doc, "select", "Select", elements);

            // Get all textarea elements
            ProcessElementsByTag(doc, "textarea", "Textarea", elements);

            // Get all anchor elements
            ProcessElementsByTag(doc, "a", "Link", elements);

            // Get all form elements
            ProcessElementsByTag(doc, "form", "Form", elements);

            // Get all div elements with id or class
            ProcessElementsByTag(doc, "div[@id or @class]", "Div", elements);

            // Get all span elements with id or class
            ProcessElementsByTag(doc, "span[@id or @class]", "Span", elements);

            return elements;
        }

        private void ProcessElementsByTag(HtmlDocument doc, string xpath, string elementType, List<WebElement> elements)
        {
            var nodes = doc.DocumentNode.SelectNodes($"//{xpath}");

            if (nodes == null) return;

            foreach (var node in nodes)
            {
                var id = node.GetAttributeValue("id", "N/A");
                var name = node.GetAttributeValue("name", "");
                var elementClass = node.GetAttributeValue("class", "");
                var elementName = !string.IsNullOrEmpty(name) ? name : id != "N/A" ? id : elementClass;

                // Create a logical name
                string displayName = GenerateElementName(elementType, elementName, id, elements.Count);

                elements.Add(new WebElement
                {
                    Name = displayName,
                    Id = id,
                    RelativeXPath = GetRelativeXPath(node),
                    FullXPath = node.XPath,
                    ElementType = elementType
                });
            }
        }

        private string GenerateElementName(string elementType, string elementName, string id, int count)
        {
            if (!string.IsNullOrWhiteSpace(elementName))
            {
                // Clean up name - remove spaces, special characters
                elementName = new string(elementName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
                return $"{elementType}{(string.IsNullOrEmpty(elementName) ? count.ToString() : elementName)}";
            }
            else if (id != "N/A")
            {
                id = new string(id.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
                return $"{elementType}{id}";
            }
            else
            {
                return $"{elementType}{count}";
            }
        }

        private string GetRelativeXPath(HtmlNode node)
        {
            try
            {
                // Get the closest ancestor with an ID
                var ancestorWithId = node.Ancestors().FirstOrDefault(a => !string.IsNullOrEmpty(a.GetAttributeValue("id", "")));

                if (ancestorWithId != null)
                {
                    // Create XPath from ancestor with ID to current node
                    var relativeXPath = $"//*[@id='{ancestorWithId.GetAttributeValue("id", "")}']";
                    var currentPath = node.XPath.Substring(ancestorWithId.XPath.Length);
                    return $"{relativeXPath}{currentPath}";
                }

                // If no ancestor with ID found, return "N/A"
                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
    }
}
