using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Readers;

public class XmlFileReader : IFileReader<XmlData>
{
    private readonly IExceptionHandler? _exceptionHandler;

    public XmlFileReader(IExceptionHandler? exceptionHandler = null)
    {
        _exceptionHandler = exceptionHandler;
    }

    public async Task<XmlData> ReadWithAutoDetectDelimiterAsync(string filePath)
    {
        return await ReadAsync(filePath);
    }

    public async Task<XmlData> ReadAsync(string filePath, object? options = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"XML file not found: {filePath}");
            }

            bool preserveWhitespace = false;

            if (options is Dictionary<string, object> optionsDict)
            {
                if (optionsDict.TryGetValue("preserveWhitespace", out var preserve) && preserve is bool preserveValue)
                {
                    preserveWhitespace = preserveValue;
                }
            }

            LoadOptions loadOptions = preserveWhitespace
                ? LoadOptions.PreserveWhitespace
                : LoadOptions.None;

            XDocument doc = await Task.Run(() => XDocument.Load(filePath, loadOptions));

            var headers = ExtractHeaders(doc);
            var rows = ExtractRows(doc, headers);

            var result = new XmlData
            {
                Document = doc,
                Headers = headers,
                Rows = rows,
                RootElementName = doc.Root?.Name.LocalName ?? "root",
                XmlVersion = doc.Declaration?.Version ?? "1.0",
                Encoding = doc.Declaration?.Encoding ?? "UTF-8"
            };

            return result;
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
        if (doc.Root == null)
            return Array.Empty<string>();

        var headers = new List<string>();

        var allElements = doc.Root.Elements().ToList();

        if (!allElements.Any())
            return Array.Empty<string>();

        foreach (var element in allElements)
        {
            foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                var name = "attr_" + attr.Name.LocalName;
                if (!headers.Contains(name)) headers.Add(name);
            }

            foreach (var child in element.Elements())
            {
                var name = child.Name.LocalName;
                if (!headers.Contains(name)) headers.Add(name);
            }

            if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            {
                if (!headers.Contains("text_value")) headers.Add("text_value");
            }
        }

        return headers.ToArray();
    }

    private List<string[]> ExtractRows(XDocument doc, string[] headers)
    {
        var rows = new List<string[]>();

        if (doc.Root == null)
            return rows;

        var allElements = doc.Root.Elements().ToList();

        foreach (var element in allElements)
        {
            var row = new string[headers.Length];

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                if (header.StartsWith("attr_"))
                {
                    var attrName = header.Substring(5);
                    var attr = element.Attributes().FirstOrDefault(a => a.Name.LocalName == attrName);
                    row[i] = attr?.Value ?? string.Empty;
                }
                else if (header == "text_value")
                {
                    if (!element.HasElements)
                    {
                        row[i] = element.Value?.Trim() ?? string.Empty;
                    }
                    else
                    {
                        row[i] = string.Empty;
                    }
                }
                else
                {
                    var childElement = element.Elements().FirstOrDefault(e => e.Name.LocalName == header);

                    if (childElement != null)
                    {
                        var cdata = childElement.DescendantNodes().OfType<XCData>().FirstOrDefault();
                        if (cdata != null)
                        {
                            row[i] = cdata.Value;
                        }
                        else
                        {
                            var repeated = childElement.Elements().ToList();
                            if (repeated.Count > 1)
                            {
                                row[i] = string.Join("; ", repeated.Select(r => r.Value?.Trim() ?? string.Empty));
                            }
                            else
                            {
                                row[i] = childElement.Value?.Trim() ?? string.Empty;
                            }
                        }
                    }
                    else
                    {
                        row[i] = string.Empty;
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

            content = content.Replace("\0", string.Empty);
            content = Regex.Replace(content, @"[^\u0009\u000A\u000D\u0020-\uFFFF]", string.Empty);

            content = WebUtility.HtmlDecode(content);

            try
            {
                var doc = XDocument.Parse(content, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

                var headers = ExtractHeaders(doc);
                var rows = ExtractRows(doc, headers);

                return new XmlData
                {
                    Document = doc,
                    Headers = headers,
                    Rows = rows,
                    RootElementName = doc.Root?.Name.LocalName ?? "root",
                    XmlVersion = doc.Declaration?.Version ?? "1.0",
                    Encoding = doc.Declaration?.Encoding ?? "UTF-8"
                };
            }
            catch (Exception parseEx)
            {
                _exceptionHandler?.Handle(
                    new Exception("XDocument.Parse failed during manual parse, falling back to regex", parseEx));

                var rows = new List<Dictionary<string, string>>();
                var allHeaders = new HashSet<string>();

                var tagPattern = @"<([a-zA-Z0-9_:\-\.]+)(?:\s+[^>]*)?>(.*?)</\1>";
                var rowMatches = Regex.Matches(content, tagPattern, RegexOptions.Singleline);

                foreach (Match rowMatch in rowMatches)
                {
                    var rowContent = rowMatch.Groups[2].Value;
                    var row = new Dictionary<string, string>();

                    var elementPattern = @"<([a-zA-Z0-9_:\-\.]+)(?:\s+[^>]*)?>(.*?)</\1>";
                    var elementMatches = Regex.Matches(rowContent, elementPattern, RegexOptions.Singleline);

                    foreach (Match elementMatch in elementMatches)
                    {
                        var elementName = elementMatch.Groups[1].Value;
                        var elementContent = elementMatch.Groups[2].Value;

                        if (elementContent.StartsWith("<![CDATA[") && elementContent.EndsWith("]]>"))
                        {
                            elementContent = elementContent.Substring(9, elementContent.Length - 12);
                        }

                        elementContent = WebUtility.HtmlDecode(elementContent);

                        row[elementName] = elementContent;
                        allHeaders.Add(elementName);
                    }

                    var attrPattern = @"\s+([a-zA-Z0-9_:\-\.]+)=(?:'([^']*)'|""([^""]*)"")";
                    var attrMatches = Regex.Matches(rowMatch.Value, attrPattern);

                    foreach (Match attrMatch in attrMatches)
                    {
                        var attrName = "attr_" + attrMatch.Groups[1].Value;
                        var attrValue = attrMatch.Groups[2].Success
                            ? attrMatch.Groups[2].Value
                            : attrMatch.Groups[3].Value;
                        row[attrName] = attrValue;
                        allHeaders.Add(attrName);
                    }

                    rows.Add(row);
                }

                var headerArray = allHeaders.OrderBy(h => h).ToArray();
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

                _exceptionHandler?.Handle(new Exception("Successfully parsed XML using manual parser (fallback)"));
                return new XmlData
                {
                    Headers = headerArray,
                    Rows = dataRows,
                    Document = new XDocument(),
                    RootElementName = "root",
                    XmlVersion = "1.0",
                    Encoding = "UTF-8"
                };
            }
        }
        catch (Exception ex)
        {
            _exceptionHandler?.Handle(new Exception("Manual XML parsing failed", ex));
            throw;
        }
    }
}