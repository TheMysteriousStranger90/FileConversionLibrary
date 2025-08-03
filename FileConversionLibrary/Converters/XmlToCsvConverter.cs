using System.Text;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class XmlToCsvConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object? options = null)
    {
        if (input?.Document?.Root == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        var delimiter = ',';
        bool quoteValues = true;
        bool includeHeaders = true;
        bool flattenHierarchy = true;
        string? customNullValue = null;

        if (options is char delimiterChar)
        {
            delimiter = delimiterChar;
        }
        else if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("delimiter", out var delimiterObj) && delimiterObj is char delimiterChar2)
            {
                delimiter = delimiterChar2;
            }

            if (optionsDict.TryGetValue("quoteValues", out var quoteObj) && quoteObj is bool quoteValue)
            {
                quoteValues = quoteValue;
            }

            if (optionsDict.TryGetValue("includeHeaders", out var headersObj) && headersObj is bool headersValue)
            {
                includeHeaders = headersValue;
            }

            if (optionsDict.TryGetValue("flattenHierarchy", out var flattenObj) && flattenObj is bool flattenValue)
            {
                flattenHierarchy = flattenValue;
            }

            if (optionsDict.TryGetValue("customNullValue", out var nullObj) && nullObj is string nullValue)
            {
                customNullValue = nullValue;
            }
        }

        var root = input.Document.Root;
        
        var recordElements = root.Elements().ToList();
        
        if (!recordElements.Any())
        {
            throw new ArgumentException("No record elements found in XML data");
        }

        var headers = ExtractHeaders(recordElements, flattenHierarchy);
        
        if (!headers.Any())
        {
            throw new ArgumentException("No headers found in XML data");
        }

        var sb = new StringBuilder();

        if (includeHeaders)
        {
            var headerLine = string.Join(delimiter.ToString(),
                headers.Select(h => quoteValues ? QuoteValue(h, delimiter) : h));
            sb.AppendLine(headerLine);
        }

        foreach (var record in recordElements)
        {
            var values = new List<string>();
            
            foreach (var header in headers)
            {
                var value = ExtractValue(record, header, flattenHierarchy);
                
                if (string.IsNullOrEmpty(value) && customNullValue != null)
                {
                    value = customNullValue;
                }
                
                values.Add(quoteValues ? QuoteValue(value ?? string.Empty, delimiter) : (value ?? string.Empty));
            }
            
            sb.AppendLine(string.Join(delimiter.ToString(), values));
        }

        return sb.ToString();
    }

    private List<string> ExtractHeaders(List<XElement> recordElements, bool flattenHierarchy)
    {
        var allHeaders = new HashSet<string>();

        foreach (var record in recordElements)
        {
            if (flattenHierarchy)
            {
                ExtractHeadersRecursive(record, allHeaders, "");
            }
            else
            {
                foreach (var element in record.Elements())
                {
                    allHeaders.Add(element.Name.LocalName);
                }
            }

            foreach (var attr in record.Attributes())
            {
                allHeaders.Add($"@{attr.Name.LocalName}");
            }
        }

        return allHeaders.OrderBy(h => h).ToList();
    }

    private void ExtractHeadersRecursive(XElement element, HashSet<string> headers, string prefix)
    {
        foreach (var child in element.Elements())
        {
            var headerName = string.IsNullOrEmpty(prefix) ? child.Name.LocalName : $"{prefix}.{child.Name.LocalName}";
            
            if (child.HasElements && !child.Elements().All(e => e.HasElements))
            {
                ExtractHeadersRecursive(child, headers, headerName);
            }
            else if (!child.HasElements)
            {
                headers.Add(headerName);
            }
            else
            {
                ExtractHeadersRecursive(child, headers, headerName);
            }
        }

        foreach (var attr in element.Attributes())
        {
            var attrName = string.IsNullOrEmpty(prefix) ? $"@{attr.Name.LocalName}" : $"{prefix}.@{attr.Name.LocalName}";
            headers.Add(attrName);
        }
    }

    private string? ExtractValue(XElement record, string path, bool flattenHierarchy)
    {
        if (path.StartsWith("@"))
        {
            var attrName = path.Substring(1);
            return record.Attribute(attrName)?.Value;
        }

        if (!flattenHierarchy || !path.Contains("."))
        {
            var element = record.Element(path);
            return element?.Value;
        }

        var parts = path.Split('.');
        var current = record;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            
            if (part.StartsWith("@"))
            {
                var attrName = part.Substring(1);
                return current?.Attribute(attrName)?.Value;
            }
            else
            {
                current = current?.Element(part);
                if (current == null)
                {
                    return null;
                }
            }
        }

        return current?.Value;
    }

    private string QuoteValue(string value, char delimiter)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }
}