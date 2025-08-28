using System.Xml;
using System.Xml.Linq;
using TestTools.Models;

namespace TestTools.Services
{
    public class XmlCompareService
    {
        public List<XmlDifference> CompareXml(string firstXml, string secondXml)
        {
            var differences = new List<XmlDifference>();
            try
            {
                XDocument firstDoc = XDocument.Parse(firstXml);
                XDocument secondDoc = XDocument.Parse(secondXml);
                CompareElements(firstDoc.Root, secondDoc.Root, "", differences);
            }
            catch (Exception ex)
            {
                differences.Add(new XmlDifference
                {
                    Path = "Error",
                    FirstValue = ex.Message,
                    SecondValue = "",
                    Type = DifferenceType.ValueDifference
                });
            }
            return differences;
        }
        private void CompareElements(XElement? first, XElement? second, string path, List<XmlDifference> differences)
        {
            if (first == null && second == null) return;
            if (first == null)
            {
                differences.Add(new XmlDifference
                {
                    Path = $"{path}/{second?.Name.LocalName}",
                    FirstValue = "",
                    SecondValue = second?.ToString(),
                    Type = DifferenceType.ExtraNode
                });
                return;
            }
            if (second == null)
            {
                differences.Add(new XmlDifference
                {
                    Path = $"{path}/{first.Name.LocalName}",
                    FirstValue = first.ToString(),
                    SecondValue = "",
                    Type = DifferenceType.MissingNode
                });
                return;
            }
            string currentPath = string.IsNullOrEmpty(path) ? first.Name.LocalName : $"{path}/{first.Name.LocalName}";
            CompareAttributes(first, second, currentPath, differences);
            if (!first.Elements().Any() && !second.Elements().Any())
            {
                string firstValue = first.Value;
                string secondValue = second.Value;
                if (firstValue != secondValue)
                {
                    differences.Add(new XmlDifference
                    {
                        Path = currentPath,
                        FirstValue = firstValue,
                        SecondValue = secondValue,
                        Type = DifferenceType.ValueDifference
                    });
                }
            }
            var firstChildren = first.Elements().ToList();
            var secondChildren = second.Elements().ToList();
            var firstDict = CreateElementDictionary(firstChildren);
            var secondDict = CreateElementDictionary(secondChildren);
            foreach (var element in firstChildren)
            {
                string key = GetElementKey(element);
                if (secondDict.TryGetValue(key, out XElement? matchingElement))
                {
                    CompareElements(element, matchingElement, currentPath, differences);
                    secondDict.Remove(key);
                }
                else
                {
                    differences.Add(new XmlDifference
                    {
                        Path = $"{currentPath}/{element.Name.LocalName}",
                        FirstValue = element.ToString(),
                        SecondValue = "",
                        Type = DifferenceType.MissingNode
                    });
                }
            }
            foreach (var element in secondChildren)
            {
                string key = GetElementKey(element);
                if (!firstDict.ContainsKey(key))
                {
                    differences.Add(new XmlDifference
                    {
                        Path = $"{currentPath}/{element.Name.LocalName}",
                        FirstValue = "",
                        SecondValue = element.ToString(),
                        Type = DifferenceType.ExtraNode
                    });
                }
            }
        }
        private void CompareAttributes(XElement first, XElement second, string path, List<XmlDifference> differences)
        {
            var firstAttrs = first.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);
            var secondAttrs = second.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);
            foreach (var attr in firstAttrs)
            {
                if (secondAttrs.TryGetValue(attr.Key, out var secondValue))
                {
                    if (attr.Value != secondValue)
                    {
                        differences.Add(new XmlDifference
                        {
                            Path = $"{path}[@{attr.Key}]",
                            FirstValue = attr.Value,
                            SecondValue = secondValue,
                            Type = DifferenceType.AttributeDifference
                        });
                    }
                    secondAttrs.Remove(attr.Key);
                }
                else
                {
                    differences.Add(new XmlDifference
                    {
                        Path = $"{path}[@{attr.Key}]",
                        FirstValue = attr.Value,
                        SecondValue = "",
                        Type = DifferenceType.MissingNode
                    });
                }
            }
            foreach (var attr in secondAttrs)
            {
                differences.Add(new XmlDifference
                {
                    Path = $"{path}[@{attr.Key}]",
                    FirstValue = "",
                    SecondValue = attr.Value,
                    Type = DifferenceType.ExtraNode
                });
            }
        }
        private Dictionary<string, XElement> CreateElementDictionary(List<XElement> elements)
        {
            var dict = new Dictionary<string, XElement>();
            foreach (var element in elements)
            {
                string key = GetElementKey(element);
                if (!dict.ContainsKey(key))
                {
                    dict[key] = element;
                }
            }
            return dict;
        }
        private string GetElementKey(XElement element)
        {
            var sb = new System.Text.StringBuilder(element.Name.LocalName);
            foreach (var attr in element.Attributes())
            {
                sb.Append($"|{attr.Name.LocalName}:{attr.Value}");
            }
            return sb.ToString();
        }
        public string HighlightDifferences(string xmlContent, List<XmlDifference> differences, bool isFirstFile)
        {
            // Implementation for highlighting differences in XML (for brevity, not shown here)
            // ...
            return xmlContent; // Placeholder
        }
        public string MergeXmlDocuments(string firstXml, string secondXml)
        {
            try
            {
                XDocument firstDoc = XDocument.Parse(firstXml);
                XDocument secondDoc = XDocument.Parse(secondXml);
                XDocument mergedDoc = new XDocument(new XElement(firstDoc.Root.Name));
                MergeElements(firstDoc.Root, secondDoc.Root, mergedDoc.Root);
                var writerSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = false
                };
                using (var sw = new System.IO.StringWriter())
                {
                    using (var writer = XmlWriter.Create(sw, writerSettings))
                    {
                        mergedDoc.Save(writer);
                    }
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error merging XML documents: {ex.Message}";
            }
        }
        private void MergeElements(XElement first, XElement second, XElement merged)
        {
            Dictionary<string, bool> processedKeys = new Dictionary<string, bool>();
            foreach (var element in first.Elements())
            {
                string key = GetElementKey(element);
                var matchingElement = second.Elements().FirstOrDefault(e => GetElementKey(e) == key);
                XElement mergedElement = new XElement(element.Name);
                foreach (var attr in element.Attributes())
                {
                    mergedElement.Add(new XAttribute(attr.Name, attr.Value));
                }
                if (matchingElement != null)
                {
                    foreach (var attr in matchingElement.Attributes())
                    {
                        if (mergedElement.Attribute(attr.Name) == null)
                        {
                            mergedElement.Add(new XAttribute(attr.Name, attr.Value));
                        }
                    }
                }
                if (element.HasElements)
                {
                    if (matchingElement != null && matchingElement.HasElements)
                    {
                        MergeElements(element, matchingElement, mergedElement);
                    }
                    else
                    {
                        foreach (var child in element.Elements())
                        {
                            mergedElement.Add(new XElement(child));
                        }
                    }
                }
                else
                {
                    string value = !string.IsNullOrEmpty(element.Value) ? element.Value : (matchingElement != null ? matchingElement.Value : string.Empty);
                    mergedElement.Value = value;
                }
                merged.Add(mergedElement);
                processedKeys[key] = true;
            }
            foreach (var element in second.Elements())
            {
                string key = GetElementKey(element);
                if (!processedKeys.ContainsKey(key))
                {
                    merged.Add(new XElement(element));
                }
            }
        }
    }
}
