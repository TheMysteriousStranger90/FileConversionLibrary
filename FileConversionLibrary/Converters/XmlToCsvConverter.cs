using System.Text;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class XmlToCsvConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object? options = null)
    {
        if (input == null)
        {
            throw new ArgumentException("Input XmlData cannot be null");
        }

        var dataFormat = DetermineDataFormat(input);

        return dataFormat switch
        {
            XmlDataFormat.Document => ConvertFromDocument(input, options),
            XmlDataFormat.TabularData => ConvertFromTabularData(input, options),
            XmlDataFormat.Both => ConvertFromDocument(input, options),
            _ => throw new ArgumentException("XmlData contains no valid data format")
        };
    }

    private enum XmlDataFormat
    {
        None,
        Document,
        TabularData,
        Both
    }

    private XmlDataFormat DetermineDataFormat(XmlData input)
    {
        bool hasDocument = input.Document?.Root != null;
        bool hasTabularData = input.Headers?.Length > 0 && input.Rows?.Count > 0;

        return (hasDocument, hasTabularData) switch
        {
            (true, true) => XmlDataFormat.Both,
            (true, false) => XmlDataFormat.Document,
            (false, true) => XmlDataFormat.TabularData,
            _ => XmlDataFormat.None
        };
    }

    private string ConvertFromDocument(XmlData input, object? options)
    {
        if (input.Document?.Root == null)
        {
            throw new ArgumentException("Invalid XML Document");
        }

        var delimiter = ',';
        bool quoteValues = true;
        bool includeHeaders = true;
        bool flattenHierarchy = true;
        string? customNullValue = null;

        ParseOptions(options, ref delimiter, ref quoteValues, ref includeHeaders,
            ref flattenHierarchy, ref customNullValue);

        var root = input.Document.Root;
        List<XElement> recordElements;
        bool isComplexStructure = false;

        recordElements = root.Elements().ToList();
        
        if (!recordElements.Any())
        {
            throw new ArgumentException("No record elements found in XML data");
        }

        if (recordElements.Any(e => e.HasElements) && 
            recordElements.Select(e => e.Name.LocalName).Distinct().Count() > 1)
        {
            isComplexStructure = true;
            recordElements = new List<XElement> { root };
        }

        bool isSingleRecord = recordElements.Select(e => e.Name.LocalName).Distinct().Count() == recordElements.Count();

        List<string> headers;
        if (isComplexStructure || isSingleRecord)
        {
            headers = ExtractFlattenedHeaders(recordElements, flattenHierarchy);
        }
        else
        {
            headers = ExtractHeaders(recordElements, flattenHierarchy);
        }
        
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

        if (isComplexStructure || isSingleRecord)
        {
            foreach (var record in recordElements)
            {
                var values = new List<string>();
                
                foreach (var header in headers)
                {
                    string? value;
                    
                    if (header.StartsWith("@"))
                    {
                        var attrName = header.Substring(1);
                        value = record.Attribute(attrName)?.Value;
                    }
                    else if (header.Contains("."))
                    {
                        value = ExtractValueFromPath(record, header);
                    }
                    else
                    {
                        var element = record.Element(header);
                        if (element != null)
                        {
                            var cdata = element.Nodes().OfType<XCData>().FirstOrDefault();
                            if (cdata != null)
                            {
                                value = cdata.Value;
                            }
                            else
                            {
                                value = element.Value;
                            }
                        }
                        else
                        {
                            value = null;
                        }
                    }
                    
                    if (string.IsNullOrEmpty(value) && customNullValue != null)
                    {
                        value = customNullValue;
                    }
                    
                    values.Add(quoteValues ? QuoteValue(value ?? string.Empty, delimiter) : (value ?? string.Empty));
                }
                
                sb.AppendLine(string.Join(delimiter.ToString(), values));
            }
        }
        else
        {
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
        }

        return sb.ToString();
    }

    private List<string> ExtractFlattenedHeaders(List<XElement> elements, bool flattenHierarchy)
    {
        var headers = new HashSet<string>();
        
        foreach (var element in elements)
        {
            foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                headers.Add($"@{attr.Name.LocalName}");
            }
            
            ExtractHeadersFromElement(element, headers, "", flattenHierarchy);
        }
        
        return headers.OrderBy(h => h).ToList();
    }
    
    private void ExtractHeadersFromElement(XElement element, HashSet<string> headers, string prefix, bool flattenHierarchy)
    {
        foreach (var child in element.Elements())
        {
            string childName = child.Name.LocalName;
            string headerName = string.IsNullOrEmpty(prefix) ? childName : $"{prefix}.{childName}";
            
            if (!child.HasElements || (!flattenHierarchy && !string.IsNullOrEmpty(child.Value.Trim())))
            {
                headers.Add(headerName);
            }
            
            foreach (var attr in child.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                headers.Add($"{headerName}.@{attr.Name.LocalName}");
            }
            
            if (child.HasElements && flattenHierarchy)
            {
                ExtractHeadersFromElement(child, headers, headerName, flattenHierarchy);
            }
            else if (child.HasElements)
            {
                headers.Add(headerName);
            }
        }
    }
    
    private string? ExtractValueFromPath(XElement element, string path)
    {
        string[] parts = path.Split('.');
        XElement? current = element;
        
        for (int i = 0; i < parts.Length - 1; i++)
        {
            string part = parts[i];
            
            if (part.StartsWith("@"))
            {
                return null;
            }
            
            current = current?.Element(part);
            
            if (current == null)
            {
                return null;
            }
        }
        
        string lastPart = parts[parts.Length - 1];
        
        if (lastPart.StartsWith("@"))
        {
            string attrName = lastPart.Substring(1);
            return current?.Attribute(attrName)?.Value;
        }
        else
        {
            var targetElement = current?.Element(lastPart);
            if (targetElement == null)
            {
                return null;
            }
            
            var cdata = targetElement.Nodes().OfType<XCData>().FirstOrDefault();
            if (cdata != null)
            {
                return cdata.Value;
            }
            
            return targetElement.Value;
        }
    }

    private string ConvertFromTabularData(XmlData input, object? options)
    {
        if (input.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid tabular data in XmlData");
        }

        if (input.Headers.Length == 0)
        {
            throw new ArgumentException("No headers found in tabular data");
        }

        var delimiter = ',';
        bool quoteValues = true;
        bool includeHeaders = true;
        bool flattenHierarchy = false;
        string? customNullValue = null;

        ParseOptions(options, ref delimiter, ref quoteValues, ref includeHeaders,
            ref flattenHierarchy, ref customNullValue);

        var sb = new StringBuilder();

        if (includeHeaders)
        {
            var headerLine = string.Join(delimiter.ToString(),
                input.Headers.Select(h => quoteValues ? QuoteValue(h, delimiter) : h));
            sb.AppendLine(headerLine);
        }

        foreach (var row in input.Rows)
        {
            var values = new string[input.Headers.Length];

            for (int i = 0; i < input.Headers.Length; i++)
            {
                string cellValue;
                if (i < row.Length)
                {
                    cellValue = row[i] ?? string.Empty;
                    if (string.IsNullOrEmpty(cellValue) && customNullValue != null)
                    {
                        cellValue = customNullValue;
                    }
                }
                else
                {
                    cellValue = customNullValue ?? string.Empty;
                }

                values[i] = quoteValues ? QuoteValue(cellValue, delimiter) : cellValue;
            }

            sb.AppendLine(string.Join(delimiter.ToString(), values));
        }

        return sb.ToString();
    }

    private void ParseOptions(object? options, ref char delimiter, ref bool quoteValues,
        ref bool includeHeaders, ref bool flattenHierarchy, ref string? customNullValue)
    {
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
            
            if (element != null)
            {
                var cdata = element.Nodes().OfType<XCData>().FirstOrDefault();
                if (cdata != null)
                {
                    return cdata.Value;
                }
            }
            
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
        
        var cdataNode = current.Nodes().OfType<XCData>().FirstOrDefault();
        if (cdataNode != null)
        {
            return cdataNode.Value;
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