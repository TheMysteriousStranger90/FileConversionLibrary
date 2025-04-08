using System.Xml;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Readers;

public class XmlFileReader : IFileReader<XmlData>
{
    private readonly IExceptionHandler _exceptionHandler;

    public XmlFileReader(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task<XmlData> ReadWithAutoDetectDelimiterAsync(string filePath)
    {
        return await ReadAsync(filePath);
    }

    private async Task<XmlData> ReadAsync(string filePath, object options = null)
    {
        try
        {
            XDocument doc = await Task.Run(() => XDocument.Load(filePath, LoadOptions.PreserveWhitespace));
            
            var headers = ExtractHeaders(doc);
            var rows = ExtractRows(doc, headers);
            
            return new XmlData { Document = doc, Headers = headers, Rows = rows };
        }
        catch (XmlException ex)
        {
            _exceptionHandler?.Handle(new Exception("Standard XML parser failed, trying manual parsing", ex));
            return await ManualParseXmlAsync(filePath);
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
    
    private string[] ExtractHeaders(XDocument doc)
    {
        var elementName = doc.Root?.Elements().FirstOrDefault()?.Name.LocalName;
        var rowElements = doc.Root?.Elements(elementName).ToList() ?? new List<XElement>();
        
        if (!rowElements.Any())
            return Array.Empty<string>();
        
        var firstRow = rowElements.First();
        
        var elementHeaders = firstRow.Elements()
            .Select(e => e.Name.LocalName)
            .ToList();
        
        var attributeHeaders = firstRow.Attributes()
            .Where(a => !a.IsNamespaceDeclaration)
            .Select(a => "attr_" + a.Name.LocalName)
            .ToList();
        
        return attributeHeaders.Concat(elementHeaders).ToArray();
    }
    
    private List<string[]> ExtractRows(XDocument doc, string[] headers)
    {
        var rows = new List<string[]>();
        
        var elementName = doc.Root?.Elements().FirstOrDefault()?.Name.LocalName;
        var rowElements = doc.Root?.Elements(elementName).ToList() ?? new List<XElement>();
        
        foreach (var rowElement in rowElements)
        {
            var row = new string[headers.Length];
            
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                
                if (header.StartsWith("attr_"))
                {
                    var attrName = header.Substring(5);
                    var attr = rowElement.Attribute(attrName);
                    row[i] = attr?.Value ?? string.Empty;
                }
                else
                {
                    var element = rowElement.Element(header);
                    
                    var cdata = element?.DescendantNodes().OfType<XCData>().FirstOrDefault();
                    if (cdata != null)
                    {
                        row[i] = cdata.Value;
                    }
                    else
                    {
                        row[i] = element?.Value ?? string.Empty;
                    }
                }
            }
            
            rows.Add(row);
        }
        
        return rows;
    }
    
    private async Task<XmlData> ManualParseXmlAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            
            var rows = new List<Dictionary<string, string>>();
            var allHeaders = new HashSet<string>();
            
            var tagPattern = @"<([a-zA-Z0-9_]+)(?:\s+[^>]*)?>(.*?)</\1>";
            var rowMatches = System.Text.RegularExpressions.Regex.Matches(content, tagPattern, 
                System.Text.RegularExpressions.RegexOptions.Singleline);
            
            foreach (System.Text.RegularExpressions.Match rowMatch in rowMatches)
            {
                var rowTag = rowMatch.Groups[1].Value;
                var rowContent = rowMatch.Groups[2].Value;
                
                var row = new Dictionary<string, string>();
                
                var elementPattern = @"<([a-zA-Z0-9_]+)(?:\s+[^>]*)?>(.*?)</\1>";
                var elementMatches = System.Text.RegularExpressions.Regex.Matches(rowContent, elementPattern, 
                    System.Text.RegularExpressions.RegexOptions.Singleline);
                
                foreach (System.Text.RegularExpressions.Match elementMatch in elementMatches)
                {
                    var elementName = elementMatch.Groups[1].Value;
                    var elementContent = elementMatch.Groups[2].Value;
                    
                    if (elementContent.StartsWith("<![CDATA[") && elementContent.EndsWith("]]>"))
                    {
                        elementContent = elementContent.Substring(9, elementContent.Length - 12);
                    }
                    
                    elementContent = DecodeXmlEntities(elementContent);
                    
                    row[elementName] = elementContent;
                    allHeaders.Add(elementName);
                }
                
                var attrPattern = @"\s+([a-zA-Z0-9_]+)=""([^""]*)""";
                var attrMatches = System.Text.RegularExpressions.Regex.Matches(rowMatch.Value, attrPattern);
                
                foreach (System.Text.RegularExpressions.Match attrMatch in attrMatches)
                {
                    var attrName = "attr_" + attrMatch.Groups[1].Value;
                    var attrValue = attrMatch.Groups[2].Value;
                    row[attrName] = attrValue;
                    allHeaders.Add(attrName);
                }
                
                rows.Add(row);
            }

            var headerArray = allHeaders.ToArray();
            var dataRows = new List<string[]>();
            
            foreach (var row in rows)
            {
                var dataRow = new string[headerArray.Length];
                for (int i = 0; i < headerArray.Length; i++)
                {
                    dataRow[i] = row.TryGetValue(headerArray[i], out var value) ? value : string.Empty;
                }
                dataRows.Add(dataRow);
            }
            
            _exceptionHandler?.Handle(new Exception("Successfully parsed XML using manual parser"));
            return new XmlData { Headers = headerArray, Rows = dataRows };
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(new Exception("Manual XML parsing failed", ex));
            throw;
        }
    }

    private string DecodeXmlEntities(string content)
    {
        return content
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'");
    }
}








/*
public class XmlFileReader : IFileReader<XmlData>
{
    private readonly IExceptionHandler _exceptionHandler;

    public XmlFileReader(IExceptionHandler exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task<XmlData> ReadWithAutoDetectDelimiterAsync(string filePath)
    {
        return await ReadAsync(filePath);
    }

    private async Task<XmlData> ReadAsync(string filePath, object options = null)
    {
        try
        {
            XDocument doc = await Task.Run(() => XDocument.Load(filePath));
            
            var headers = ExtractHeaders(doc);
            var rows = ExtractRows(doc, headers);
            
            return new XmlData { Document = doc, Headers = headers, Rows = rows };
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(ex);
            throw;
        }
    }
    
    private string[] ExtractHeaders(XDocument doc)
    {
        var elementName = doc.Root?.Elements().FirstOrDefault()?.Name.LocalName;
        var rowElements = doc.Root?.Elements(elementName).ToList() ?? new List<XElement>();
        
        if (!rowElements.Any())
            return Array.Empty<string>();
        
        var firstRow = rowElements.First();
        return firstRow.Elements()
            .Select(e => e.Name.LocalName)
            .ToArray();
    }
    
    private List<string[]> ExtractRows(XDocument doc, string[] headers)
    {
        var rows = new List<string[]>();
        
        var elementName = doc.Root?.Elements().FirstOrDefault()?.Name.LocalName;
        var rowElements = doc.Root?.Elements(elementName).ToList() ?? new List<XElement>();
        
        foreach (var rowElement in rowElements)
        {
            var row = new string[headers.Length];
            
            for (int i = 0; i < headers.Length; i++)
            {
                var element = rowElement.Element(headers[i]);
                row[i] = element?.Value ?? string.Empty;
            }
            
            rows.Add(row);
        }
        
        return rows;
    }
}
*/