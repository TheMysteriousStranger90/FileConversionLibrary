using System.Text;
using System.Xml.Linq;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using System.Globalization;

namespace FileConversionLibrary.Converters;

public class CsvToXmlConverter : IConverter<CsvData, string>
{
    public enum XmlOutputFormat
    {
        Elements,
        Attributes,
        Mixed,
        Hierarchical
    }

    private enum XmlNamingConvention
    {
        Original,
        CamelCase,
        PascalCase,
        KebabCase,
        SnakeCase
    }

    public string Convert(CsvData input, object? options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }

        if (input.Headers.Length == 0)
        {
            throw new ArgumentException("No headers found in CSV data");
        }

        var outputFormat = XmlOutputFormat.Elements;
        bool useCData = true;
        string rootElementName = "root";
        string rowElementName = "row";
        bool includeTimestamp = true;
        bool includeRowNumbers = false;
        bool validateXmlNames = true;
        XmlNamingConvention namingConvention = XmlNamingConvention.Original;
        bool addXmlDeclaration = true;
        string xmlVersion = "1.0";
        string encoding = "UTF-8";
        bool addComments = true;
        bool preserveWhitespace = false;
        bool convertDataTypes = false;
        Dictionary<string, string>? customNamespaces = null;
        bool groupByColumn = false;
        string? groupByColumnName = null;
        bool sortRows = false;
        string? sortByColumn = null;
        bool ascending = true;

        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("format", out var format) && format is XmlOutputFormat fmt)
            {
                outputFormat = fmt;
            }

            if (optionsDict.TryGetValue("useCData", out var cdata) && cdata is bool cdataValue)
            {
                useCData = cdataValue;
            }

            if (optionsDict.TryGetValue("rootElementName", out var root) && root is string rootValue)
            {
                rootElementName = rootValue;
            }

            if (optionsDict.TryGetValue("rowElementName", out var rowName) && rowName is string rowNameValue)
            {
                rowElementName = rowNameValue;
            }

            if (optionsDict.TryGetValue("includeTimestamp", out var timestamp) && timestamp is bool timestampValue)
            {
                includeTimestamp = timestampValue;
            }

            if (optionsDict.TryGetValue("includeRowNumbers", out var rowNums) && rowNums is bool rowNumsValue)
            {
                includeRowNumbers = rowNumsValue;
            }

            if (optionsDict.TryGetValue("validateXmlNames", out var validate) && validate is bool validateValue)
            {
                validateXmlNames = validateValue;
            }

            if (optionsDict.TryGetValue("namingConvention", out var naming) &&
                naming is XmlNamingConvention namingValue)
            {
                namingConvention = namingValue;
            }

            if (optionsDict.TryGetValue("addXmlDeclaration", out var declaration) &&
                declaration is bool declarationValue)
            {
                addXmlDeclaration = declarationValue;
            }

            if (optionsDict.TryGetValue("xmlVersion", out var version) && version is string versionValue)
            {
                xmlVersion = versionValue;
            }

            if (optionsDict.TryGetValue("encoding", out var enc) && enc is string encValue)
            {
                encoding = encValue;
            }

            if (optionsDict.TryGetValue("addComments", out var comments) && comments is bool commentsValue)
            {
                addComments = commentsValue;
            }

            if (optionsDict.TryGetValue("preserveWhitespace", out var whitespace) && whitespace is bool whitespaceValue)
            {
                preserveWhitespace = whitespaceValue;
            }

            if (optionsDict.TryGetValue("convertDataTypes", out var convert) && convert is bool convertValue)
            {
                convertDataTypes = convertValue;
            }

            if (optionsDict.TryGetValue("customNamespaces", out var namespaces) &&
                namespaces is Dictionary<string, string> namespacesValue)
            {
                customNamespaces = namespacesValue;
            }

            if (optionsDict.TryGetValue("groupByColumn", out var group) && group is bool groupValue)
            {
                groupByColumn = groupValue;
            }

            if (optionsDict.TryGetValue("groupByColumnName", out var groupCol) && groupCol is string groupColValue)
            {
                groupByColumnName = groupColValue;
            }

            if (optionsDict.TryGetValue("sortRows", out var sort) && sort is bool sortValue)
            {
                sortRows = sortValue;
            }

            if (optionsDict.TryGetValue("sortByColumn", out var sortCol) && sortCol is string sortColValue)
            {
                sortByColumn = sortColValue;
            }

            if (optionsDict.TryGetValue("ascending", out var asc) && asc is bool ascValue)
            {
                ascending = ascValue;
            }
        }

        XDocument doc;

        if (addXmlDeclaration)
        {
            doc = new XDocument(
                new XDeclaration(xmlVersion, encoding, preserveWhitespace ? "yes" : null)
            );
        }
        else
        {
            doc = new XDocument();
        }

        var rootElement = new XElement(SanitizeXmlElementName(rootElementName, validateXmlNames, namingConvention));

        if (customNamespaces != null)
        {
            foreach (var ns in customNamespaces)
            {
                rootElement.Add(new XAttribute(XNamespace.Xmlns + ns.Key, ns.Value));
            }
        }
/*
        if (includeTimestamp)
        {
            rootElement.SetAttributeValue("generated", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        rootElement.SetAttributeValue("totalRows", input.Rows.Count);
        rootElement.SetAttributeValue("totalColumns", input.Headers.Length);
*/
        doc.Add(rootElement);

        if (groupByColumn && !string.IsNullOrEmpty(groupByColumnName))
        {
            ProcessGroupedData(rootElement, input, groupByColumnName, outputFormat, useCData,
                rowElementName, includeRowNumbers, validateXmlNames, namingConvention, convertDataTypes);
        }
        else if (outputFormat == XmlOutputFormat.Hierarchical)
        {
            ProcessHierarchicalData(rootElement, input, useCData, includeRowNumbers,
                validateXmlNames, namingConvention, convertDataTypes, sortRows, sortByColumn, ascending);
        }
        else
        {
            ProcessRegularData(rootElement, input, outputFormat, useCData, rowElementName,
                includeRowNumbers, validateXmlNames, namingConvention, convertDataTypes,
                sortRows, sortByColumn, ascending);
        }

        return doc.ToString(preserveWhitespace ? SaveOptions.None : SaveOptions.None);
    }

    private void ProcessGroupedData(XElement rootElement, CsvData input, string groupByColumnName,
        XmlOutputFormat outputFormat, bool useCData, string rowElementName, bool includeRowNumbers,
        bool validateXmlNames, XmlNamingConvention namingConvention, bool convertDataTypes)
    {
        var groupColumnIndex = Array.IndexOf(input.Headers, groupByColumnName);
        if (groupColumnIndex == -1)
        {
            throw new ArgumentException($"Group column '{groupByColumnName}' not found in headers");
        }

        var groups = new Dictionary<string, List<string[]>>();

        foreach (var row in input.Rows)
        {
            if (groupColumnIndex >= row.Length) continue;

            var groupValue = row[groupColumnIndex] ?? "null";
            if (!groups.ContainsKey(groupValue))
            {
                groups[groupValue] = new List<string[]>();
            }

            groups[groupValue].Add(row);
        }

        foreach (var group in groups)
        {
            var groupElement =
                new XElement(SanitizeXmlElementName($"group_{group.Key}", validateXmlNames, namingConvention));
            groupElement.SetAttributeValue("name", group.Key);
            groupElement.SetAttributeValue("count", group.Value.Count);

            ProcessRowsInGroup(groupElement, input, group.Value, outputFormat, useCData, rowElementName,
                includeRowNumbers, validateXmlNames, namingConvention, convertDataTypes, groupColumnIndex);

            rootElement.Add(groupElement);
        }
    }

    private void ProcessRowsInGroup(XElement groupElement, CsvData input, List<string[]> rows,
        XmlOutputFormat outputFormat, bool useCData, string rowElementName, bool includeRowNumbers,
        bool validateXmlNames, XmlNamingConvention namingConvention, bool convertDataTypes, int skipColumnIndex)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowElement = CreateRowElement(rowElementName, validateXmlNames, namingConvention);

            if (includeRowNumbers)
            {
                rowElement.SetAttributeValue("number", i + 1);
            }

            var attributeHeaders = DetermineAttributeFields(input, outputFormat, skipColumnIndex);

            ProcessRowData(rowElement, input.Headers, row, outputFormat, useCData, attributeHeaders,
                validateXmlNames, namingConvention, convertDataTypes, skipColumnIndex);

            groupElement.Add(rowElement);
        }
    }

    private void ProcessHierarchicalData(XElement rootElement, CsvData input, bool useCData,
        bool includeRowNumbers, bool validateXmlNames, XmlNamingConvention namingConvention,
        bool convertDataTypes, bool sortRows, string? sortByColumn, bool ascending)
    {
        var rowsToProcess = GetSortedRows(input, sortRows, sortByColumn, ascending);

        foreach (var (row, index) in rowsToProcess.Select((r, i) => (r, i)))
        {
            var recordElement = new XElement(SanitizeXmlElementName("record", validateXmlNames, namingConvention));

            if (includeRowNumbers)
            {
                recordElement.SetAttributeValue("id", index + 1);
            }

            for (int j = 0; j < input.Headers.Length && j < row.Length; j++)
            {
                var header = input.Headers[j];
                var value = row[j];

                if (string.IsNullOrEmpty(value)) continue;

                var fieldElement = new XElement(SanitizeXmlElementName(header, validateXmlNames, namingConvention));

                var processedValue = ProcessFieldValue(value, convertDataTypes);
                if (processedValue is string strValue && useCData && NeedsCData(strValue))
                {
                    fieldElement.Add(new XCData(strValue));
                }
                else
                {
                    fieldElement.SetValue(processedValue?.ToString() ?? string.Empty);
                }

                if (convertDataTypes && processedValue != null)
                {
                    var dataType = GetDataType(processedValue);
                    if (dataType != "string")
                    {
                        fieldElement.SetAttributeValue("type", dataType);
                    }
                }

                recordElement.Add(fieldElement);
            }

            rootElement.Add(recordElement);
        }
    }

    private void ProcessRegularData(XElement rootElement, CsvData input, XmlOutputFormat outputFormat,
        bool useCData, string rowElementName, bool includeRowNumbers, bool validateXmlNames,
        XmlNamingConvention namingConvention, bool convertDataTypes, bool sortRows, string? sortByColumn,
        bool ascending)
    {
        var attributeHeaders = DetermineAttributeFields(input, outputFormat);
        var rowsToProcess = GetSortedRows(input, sortRows, sortByColumn, ascending);

        foreach (var (row, index) in rowsToProcess.Select((r, i) => (r, i)))
        {
            var rowElement = CreateRowElement(rowElementName, validateXmlNames, namingConvention);

            if (includeRowNumbers)
            {
                rowElement.SetAttributeValue("number", index + 1);
            }

            ProcessRowData(rowElement, input.Headers, row, outputFormat, useCData, attributeHeaders,
                validateXmlNames, namingConvention, convertDataTypes);

            rootElement.Add(rowElement);
        }
    }

    private List<string[]> GetSortedRows(CsvData input, bool sortRows, string? sortByColumn, bool ascending)
    {
        if (!sortRows || string.IsNullOrEmpty(sortByColumn))
        {
            return input.Rows;
        }

        var sortColumnIndex = Array.IndexOf(input.Headers, sortByColumn);
        if (sortColumnIndex == -1)
        {
            return input.Rows;
        }

        return ascending
            ? input.Rows.OrderBy(row => sortColumnIndex < row.Length ? row[sortColumnIndex] : string.Empty).ToList()
            : input.Rows.OrderByDescending(row => sortColumnIndex < row.Length ? row[sortColumnIndex] : string.Empty)
                .ToList();
    }

    private void ProcessRowData(XElement rowElement, string[] headers, string[] row, XmlOutputFormat outputFormat,
        bool useCData, HashSet<int> attributeHeaders, bool validateXmlNames, XmlNamingConvention namingConvention,
        bool convertDataTypes, int skipColumnIndex = -1)
    {
        if (outputFormat != XmlOutputFormat.Elements)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                if (i == skipColumnIndex || !attributeHeaders.Contains(i) || i >= row.Length) continue;

                var header = headers[i];
                var value = row[i];

                if (string.IsNullOrEmpty(value)) continue;

                var processedValue = ProcessFieldValue(value, convertDataTypes);
                rowElement.SetAttributeValue(
                    SanitizeXmlElementName(header, validateXmlNames, namingConvention),
                    EscapeXmlAttributeValue(processedValue?.ToString() ?? string.Empty)
                );
            }
        }

        for (var i = 0; i < headers.Length; i++)
        {
            if (i == skipColumnIndex || attributeHeaders.Contains(i) || i >= row.Length) continue;

            var header = headers[i];
            var value = row[i];

            var fieldElement = new XElement(SanitizeXmlElementName(header, validateXmlNames, namingConvention));

            if (string.IsNullOrEmpty(value))
            {
                if (convertDataTypes)
                {
                    fieldElement.SetAttributeValue("nil", "true");
                }
            }
            else
            {
                var processedValue = ProcessFieldValue(value, convertDataTypes);

                if (processedValue is string strValue && useCData && NeedsCData(strValue))
                {
                    fieldElement.Add(new XCData(strValue));
                }
                else
                {
                    fieldElement.SetValue(processedValue?.ToString() ?? string.Empty);
                }

                if (convertDataTypes && processedValue != null)
                {
                    var dataType = GetDataType(processedValue);
                    if (dataType != "string")
                    {
                        fieldElement.SetAttributeValue("type", dataType);
                    }
                }
            }

            rowElement.Add(fieldElement);
        }
    }

    private XElement CreateRowElement(string rowElementName, bool validateXmlNames,
        XmlNamingConvention namingConvention)
    {
        return new XElement(SanitizeXmlElementName(rowElementName, validateXmlNames, namingConvention));
    }

    private HashSet<int> DetermineAttributeFields(CsvData input, XmlOutputFormat format, int skipColumnIndex = -1)
    {
        var result = new HashSet<int>();

        if (format == XmlOutputFormat.Elements)
            return result;

        for (int i = 0; i < input.Headers.Length; i++)
        {
            if (i == skipColumnIndex) continue;

            if (format == XmlOutputFormat.Attributes)
            {
                result.Add(i);
                continue;
            }

            bool isAttributeCandidate = true;
            int maxLength = 0;
            bool hasSpecialChars = false;

            foreach (var row in input.Rows)
            {
                if (i < row.Length && !string.IsNullOrEmpty(row[i]))
                {
                    var value = row[i];
                    maxLength = Math.Max(maxLength, value.Length);

                    if (value.Contains('\n') || value.Contains('\r') ||
                        value.Contains('<') || value.Contains('>') ||
                        value.Length > 80)
                    {
                        hasSpecialChars = true;
                        break;
                    }
                }
            }

            isAttributeCandidate = isAttributeCandidate && !hasSpecialChars && maxLength <= 80;

            if (isAttributeCandidate)
            {
                result.Add(i);
            }
        }

        return result;
    }

    private object ProcessFieldValue(string value, bool convertDataTypes)
    {
        if (!convertDataTypes || string.IsNullOrEmpty(value))
            return value;

        if (int.TryParse(value, out var intValue))
            return intValue;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
            return doubleValue;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        if (DateTime.TryParse(value, out var dateValue))
            return dateValue;

        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        return value;
    }

    private string GetDataType(object value)
    {
        return value switch
        {
            int => "int",
            double => "double",
            decimal => "decimal",
            DateTime => "datetime",
            bool => "boolean",
            _ => "string"
        };
    }

    private bool NeedsCData(string value)
    {
        return value.Contains('<') || value.Contains('>') ||
               value.Contains('&') || value.Contains('\n') ||
               value.Contains("]]>");
    }

    private string SanitizeXmlElementName(string name, bool validate, XmlNamingConvention convention)
    {
        if (string.IsNullOrEmpty(name))
            return "Field";

        var result = ApplyNamingConvention(name.Trim(), convention);

        if (!validate)
            return result;

        if (!char.IsLetter(result[0]) && result[0] != '_')
        {
            result = "_" + result;
        }

        var sanitized = new StringBuilder();
        foreach (char c in result)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')
                sanitized.Append(c);
            else
                sanitized.Append('_');
        }

        return sanitized.ToString();
    }

    private string ApplyNamingConvention(string name, XmlNamingConvention convention)
    {
        return convention switch
        {
            XmlNamingConvention.CamelCase => ToCamelCase(name),
            XmlNamingConvention.PascalCase => ToPascalCase(name),
            XmlNamingConvention.KebabCase => ToKebabCase(name),
            XmlNamingConvention.SnakeCase => ToSnakeCase(name),
            _ => name
        };
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = words[0].ToLower();
        for (int i = 1; i < words.Length; i++)
        {
            result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }

        return result;
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = string.Empty;
        foreach (var word in words)
        {
            result += char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }

        return result;
    }

    private string ToKebabCase(string input)
    {
        return input.ToLower().Replace(' ', '-');
    }

    private string ToSnakeCase(string input)
    {
        return input.ToLower().Replace(' ', '_');
    }

    private string EscapeXmlAttributeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}