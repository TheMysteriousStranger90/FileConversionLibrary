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

        var finalHeaders = new HashSet<string>();
        var rowElementName = FindRowElement(doc);

        if (rowElementName != null)
        {
            var rowElements = doc.Descendants(rowElementName).ToList();
            foreach (var rowElement in rowElements)
            {
                var parent = rowElement.Parent;
                while (parent != null && parent != doc.Root)
                {
                    foreach (var attr in parent.Attributes().Where(a => !a.IsNamespaceDeclaration))
                    {
                        finalHeaders.Add($"{parent.Name.LocalName}_attr_{attr.Name.LocalName}");
                    }

                    parent = parent.Parent;
                }

                foreach (var attr in rowElement.Attributes().Where(a => !a.IsNamespaceDeclaration))
                {
                    finalHeaders.Add($"attr_{attr.Name.LocalName}");
                }

                foreach (var child in rowElement.Elements())
                {
                    finalHeaders.Add(child.Name.LocalName);
                }
            }
        }

        if (!finalHeaders.Any())
        {
            return doc.Descendants().Where(e => !e.HasElements && !string.IsNullOrWhiteSpace(e.Value))
                .Select(e => e.Name.LocalName).Distinct().ToArray();
        }

        return finalHeaders.OrderBy(h => h).ToArray();
    }

    private List<string[]> ExtractRows(XDocument doc, string[] headers)
    {
        var rows = new List<string[]>();
        if (doc.Root == null) return rows;

        var rowElementName = FindRowElement(doc);
        if (rowElementName == null) return rows;

        var rowElements = doc.Descendants(rowElementName).ToList();

        foreach (var element in rowElements)
        {
            var row = new string[headers.Length];
            var rowValues = new Dictionary<string, string>();

            var parent = element.Parent;
            while (parent != null && parent != doc.Root)
            {
                foreach (var attr in parent.Attributes().Where(a => !a.IsNamespaceDeclaration))
                {
                    rowValues[$"{parent.Name.LocalName}_attr_{attr.Name.LocalName}"] = attr.Value;
                }

                parent = parent.Parent;
            }

            foreach (var attr in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                rowValues[$"attr_{attr.Name.LocalName}"] = attr.Value;
            }

            foreach (var child in element.Elements())
            {
                var repeatingChildren = child.Elements().ToList();
                if (repeatingChildren.Any() && repeatingChildren.All(e => e.Name == repeatingChildren.First().Name))
                {
                    rowValues[child.Name.LocalName] = string.Join("; ", repeatingChildren.Select(c => c.Value.Trim()));
                }
                else
                {
                    rowValues[child.Name.LocalName] = child.Value.Trim();
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                row[i] = rowValues.TryGetValue(headers[i], out var value) ? value : string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }

    private string? FindRowElement(XDocument doc)
    {
        if (doc.Root == null) return null;

        var potentialRowParents = doc.Descendants()
            .Where(p => p.Elements().Count() > 1)
            .Select(p => new
            {
                Parent = p,
                Groups = p.Elements().GroupBy(e => e.Name.LocalName)
            })
            .Where(x => x.Groups.Any(g => g.Count() > 1))
            .OrderByDescending(x => x.Groups.Max(g => g.Count()))
            .FirstOrDefault();

        if (potentialRowParents != null)
        {
            return potentialRowParents.Groups.OrderByDescending(g => g.Count()).First().Key;
        }

        var elementCounts = doc.Root.Descendants()
            .Where(e => e.HasElements)
            .GroupBy(e => e.Name.LocalName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .Where(x => x.Count > 1)
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        return elementCounts?.Name;
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