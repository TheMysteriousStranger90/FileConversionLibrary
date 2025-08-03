using System.Text;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;
using System.Globalization;

namespace FileConversionLibrary.Converters;

public class CsvToYamlConverter : IConverter<CsvData, string>
{
    private enum YamlStyle
    {
        Default,
        Flow,
        Block,
        Literal,
        Folded
    }

    private enum YamlNamingConvention
    {
        Original,
        CamelCase,
        SnakeCase,
        KebabCase,
        LowerCase
    }

    private enum YamlStructure
    {
        Array,
        Dictionary,
        Hierarchical,
        Grouped
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

        YamlStructure structure = YamlStructure.Array;
        YamlNamingConvention namingConvention = YamlNamingConvention.Original;
        bool convertDataTypes = true;
        bool includeComments = true;
        bool includeMetadata = true;
        bool preserveQuotes = false;
        int indentSize = 2;
        bool useFlowStyle = false;
        bool sortKeys = false;
        bool includeNullValues = false;
        string? groupByColumn = null;
        bool addTimestamp = true;
        string? customRootKey = null;
        bool compactArrays = false;
        string? dateFormat = null;
        bool escapeSpecialChars = true;
        Dictionary<string, object>? customMetadata = null;

        if (options is Dictionary<string, object> optionsDict)
        {
            if (optionsDict.TryGetValue("structure", out var structObj) && structObj is YamlStructure structValue)
            {
                structure = structValue;
            }

            if (optionsDict.TryGetValue("namingConvention", out var naming) &&
                naming is YamlNamingConvention namingValue)
            {
                namingConvention = namingValue;
            }

            if (optionsDict.TryGetValue("convertDataTypes", out var convert) && convert is bool convertValue)
            {
                convertDataTypes = convertValue;
            }

            if (optionsDict.TryGetValue("includeComments", out var comments) && comments is bool commentsValue)
            {
                includeComments = commentsValue;
            }

            if (optionsDict.TryGetValue("includeMetadata", out var metadata) && metadata is bool metadataValue)
            {
                includeMetadata = metadataValue;
            }

            if (optionsDict.TryGetValue("preserveQuotes", out var quotes) && quotes is bool quotesValue)
            {
                preserveQuotes = quotesValue;
            }

            if (optionsDict.TryGetValue("indentSize", out var indent) && indent is int indentValue)
            {
                indentSize = Math.Max(1, Math.Min(8, indentValue));
            }

            if (optionsDict.TryGetValue("useFlowStyle", out var flow) && flow is bool flowValue)
            {
                useFlowStyle = flowValue;
            }

            if (optionsDict.TryGetValue("sortKeys", out var sort) && sort is bool sortValue)
            {
                sortKeys = sortValue;
            }

            if (optionsDict.TryGetValue("includeNullValues", out var nulls) && nulls is bool nullsValue)
            {
                includeNullValues = nullsValue;
            }

            if (optionsDict.TryGetValue("groupByColumn", out var group) && group is string groupValue)
            {
                groupByColumn = groupValue;
                structure = YamlStructure.Grouped;
            }

            if (optionsDict.TryGetValue("addTimestamp", out var timestamp) && timestamp is bool timestampValue)
            {
                addTimestamp = timestampValue;
            }

            if (optionsDict.TryGetValue("customRootKey", out var rootKey) && rootKey is string rootKeyValue)
            {
                customRootKey = rootKeyValue;
            }

            if (optionsDict.TryGetValue("compactArrays", out var compact) && compact is bool compactValue)
            {
                compactArrays = compactValue;
            }

            if (optionsDict.TryGetValue("dateFormat", out var dateFormatObj) && dateFormatObj is string dateFormatValue)
            {
                dateFormat = dateFormatValue;
            }

            if (optionsDict.TryGetValue("escapeSpecialChars", out var escape) && escape is bool escapeValue)
            {
                escapeSpecialChars = escapeValue;
            }

            if (optionsDict.TryGetValue("customMetadata", out var customMeta) &&
                customMeta is Dictionary<string, object> customMetaValue)
            {
                customMetadata = customMetaValue;
            }
        }

        var sb = new StringBuilder();
        
        if (includeMetadata)
        {
            //AddMetadata(sb, input, addTimestamp, customMetadata, indentSize);
        }

        switch (structure)
        {
            case YamlStructure.Dictionary:
                ConvertToDictionary(sb, input, namingConvention, convertDataTypes, indentSize,
                    useFlowStyle, sortKeys, includeNullValues, dateFormat, escapeSpecialChars, preserveQuotes);
                break;

            case YamlStructure.Hierarchical:
                ConvertToHierarchical(sb, input, namingConvention, convertDataTypes, indentSize,
                    sortKeys, includeNullValues, customRootKey, dateFormat, escapeSpecialChars, preserveQuotes);
                break;

            case YamlStructure.Grouped:
                ConvertToGrouped(sb, input, groupByColumn, namingConvention, convertDataTypes,
                    indentSize, sortKeys, includeNullValues, dateFormat, escapeSpecialChars, preserveQuotes);
                break;

            case YamlStructure.Array:
            default:
                ConvertToArray(sb, input, namingConvention, convertDataTypes, indentSize,
                    useFlowStyle, sortKeys, includeNullValues, compactArrays, dateFormat, escapeSpecialChars,
                    preserveQuotes);
                break;
        }

        return sb.ToString();
    }
/*
    private void AddMetadata(StringBuilder sb, CsvData input, bool addTimestamp,
        Dictionary<string, object>? customMetadata, int indentSize)
    {
        sb.AppendLine("metadata:");

        if (addTimestamp)
        {
            sb.AppendLine($"{GetIndent(indentSize)}generated: {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}");
        }

        sb.AppendLine($"{GetIndent(indentSize)}source: CSV");
        sb.AppendLine($"{GetIndent(indentSize)}rows: {input.Rows.Count}");
        sb.AppendLine($"{GetIndent(indentSize)}columns: {input.Headers.Length}");

        sb.AppendLine($"{GetIndent(indentSize)}schema:");
        foreach (var header in input.Headers)
        {
            var dataType = DetectColumnDataType(input, header);
            sb.AppendLine(
                $"{GetIndent(indentSize * 2)}{FormatYamlKey(header, YamlNamingConvention.Original)}: {dataType}");
        }

        if (customMetadata != null)
        {
            sb.AppendLine($"{GetIndent(indentSize)}custom:");
            foreach (var kvp in customMetadata)
            {
                sb.AppendLine(
                    $"{GetIndent(indentSize * 2)}{kvp.Key}: {FormatYamlValue(kvp.Value, false, null, false, false)}");
            }
        }

        sb.AppendLine();
    }
*/
    private void ConvertToArray(StringBuilder sb, CsvData input, YamlNamingConvention namingConvention,
        bool convertDataTypes, int indentSize, bool useFlowStyle, bool sortKeys, bool includeNullValues,
        bool compactArrays, string? dateFormat, bool escapeSpecialChars, bool preserveQuotes)
    {
        sb.AppendLine("---");

        var multilineColumns = DetectMultilineColumns(input);
        var headers = sortKeys ? input.Headers.OrderBy(h => h).ToArray() : input.Headers;

        foreach (var row in input.Rows)
        {
            if (useFlowStyle && !compactArrays)
            {
                var flowItems = new List<string>();
                for (int i = 0; i < headers.Length && i < row.Length; i++)
                {
                    var header = FormatYamlKey(headers[i], namingConvention);
                    var value = ProcessValue(row[i], convertDataTypes, dateFormat, escapeSpecialChars);
                    if (value != null || includeNullValues)
                    {
                        var formattedValue = FormatYamlValue(value, multilineColumns.Contains(i),
                            dateFormat, escapeSpecialChars, preserveQuotes);
                        flowItems.Add($"{header}: {formattedValue}");
                    }
                }

                sb.AppendLine($"{GetIndent(indentSize)}{{{string.Join(", ", flowItems)}}}");
            }
            else
            {
                sb.AppendLine($"{GetIndent(indentSize)}-");

                for (int i = 0; i < headers.Length && i < row.Length; i++)
                {
                    var header = FormatYamlKey(headers[i], namingConvention);
                    var value = ProcessValue(row[i], convertDataTypes, dateFormat, escapeSpecialChars);

                    if (value == null && !includeNullValues)
                        continue;

                    var yamlStyle = DetermineYamlStyle(value?.ToString(), multilineColumns.Contains(i));
                    var formattedValue = FormatYamlValue(value, multilineColumns.Contains(i),
                        dateFormat, escapeSpecialChars, preserveQuotes);

                    switch (yamlStyle)
                    {
                        case YamlStyle.Literal:
                            sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: |");
                            var literalLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                            foreach (var line in literalLines)
                            {
                                sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                            }

                            break;

                        case YamlStyle.Folded:
                            sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: >");
                            var foldedLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                            foreach (var line in foldedLines)
                            {
                                sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                            }

                            break;

                        default:
                            sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: {formattedValue}");
                            break;
                    }
                }
            }
        }
    }

    private void ConvertToDictionary(StringBuilder sb, CsvData input, YamlNamingConvention namingConvention,
        bool convertDataTypes, int indentSize, bool useFlowStyle, bool sortKeys, bool includeNullValues,
        string? dateFormat, bool escapeSpecialChars, bool preserveQuotes)
    {
        sb.AppendLine("records:");

        for (int rowIndex = 0; rowIndex < input.Rows.Count; rowIndex++)
        {
            var row = input.Rows[rowIndex];
            sb.AppendLine($"{GetIndent(indentSize)}record_{rowIndex + 1}:");

            var headers = sortKeys ? input.Headers.OrderBy(h => h).ToArray() : input.Headers;
            var multilineColumns = DetectMultilineColumns(input);

            for (int i = 0; i < headers.Length && i < row.Length; i++)
            {
                var originalIndex = Array.IndexOf(input.Headers, headers[i]);
                var header = FormatYamlKey(headers[i], namingConvention);
                var value = ProcessValue(row[originalIndex], convertDataTypes, dateFormat, escapeSpecialChars);

                if (value == null && !includeNullValues)
                    continue;

                var yamlStyle = DetermineYamlStyle(value?.ToString(), multilineColumns.Contains(originalIndex));
                var formattedValue = FormatYamlValue(value, multilineColumns.Contains(originalIndex),
                    dateFormat, escapeSpecialChars, preserveQuotes);

                switch (yamlStyle)
                {
                    case YamlStyle.Literal:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: |");
                        var literalLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                        foreach (var line in literalLines)
                        {
                            sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                        }

                        break;

                    case YamlStyle.Folded:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: >");
                        var foldedLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                        foreach (var line in foldedLines)
                        {
                            sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                        }

                        break;

                    default:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}{header}: {formattedValue}");
                        break;
                }
            }
        }
    }

    private void ConvertToHierarchical(StringBuilder sb, CsvData input, YamlNamingConvention namingConvention,
        bool convertDataTypes, int indentSize, bool sortKeys, bool includeNullValues, string? customRootKey,
        string? dateFormat, bool escapeSpecialChars, bool preserveQuotes)
    {
        var rootKey = customRootKey ?? "dataset";
        sb.AppendLine($"{rootKey}:");

        var headers = sortKeys ? input.Headers.OrderBy(h => h).ToArray() : input.Headers;
        var multilineColumns = DetectMultilineColumns(input);

        foreach (var header in headers)
        {
            var headerIndex = Array.IndexOf(input.Headers, header);
            var formattedHeader = FormatYamlKey(header, namingConvention);

            sb.AppendLine($"{GetIndent(indentSize)}{formattedHeader}:");

            for (int rowIndex = 0; rowIndex < input.Rows.Count; rowIndex++)
            {
                var row = input.Rows[rowIndex];
                if (headerIndex >= row.Length) continue;

                var value = ProcessValue(row[headerIndex], convertDataTypes, dateFormat, escapeSpecialChars);
                if (value == null && !includeNullValues) continue;

                var yamlStyle = DetermineYamlStyle(value?.ToString(), multilineColumns.Contains(headerIndex));
                var formattedValue = FormatYamlValue(value, multilineColumns.Contains(headerIndex),
                    dateFormat, escapeSpecialChars, preserveQuotes);

                switch (yamlStyle)
                {
                    case YamlStyle.Literal:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}- |");
                        var literalLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                        foreach (var line in literalLines)
                        {
                            sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                        }

                        break;

                    case YamlStyle.Folded:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}- >");
                        var foldedLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                        foreach (var line in foldedLines)
                        {
                            sb.AppendLine($"{GetIndent(indentSize * 3)}{line}");
                        }

                        break;

                    default:
                        sb.AppendLine($"{GetIndent(indentSize * 2)}- {formattedValue}");
                        break;
                }
            }
        }
    }

    private void ConvertToGrouped(StringBuilder sb, CsvData input, string? groupByColumn,
        YamlNamingConvention namingConvention, bool convertDataTypes, int indentSize, bool sortKeys,
        bool includeNullValues, string? dateFormat, bool escapeSpecialChars, bool preserveQuotes)
    {
        if (string.IsNullOrEmpty(groupByColumn))
        {
            ConvertToArray(sb, input, namingConvention, convertDataTypes, indentSize, false,
                sortKeys, includeNullValues, false, dateFormat, escapeSpecialChars, preserveQuotes);
            return;
        }

        var groupColumnIndex = Array.IndexOf(input.Headers, groupByColumn);
        if (groupColumnIndex == -1)
        {
            throw new ArgumentException($"Group column '{groupByColumn}' not found in headers");
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

        sb.AppendLine("groups:");

        var sortedGroups = sortKeys
            ? groups.OrderBy(g => g.Key).ToList()
            : groups.ToList();

        var multilineColumns = DetectMultilineColumns(input);

        foreach (var group in sortedGroups)
        {
            var groupKey = FormatYamlKey(group.Key, namingConvention);
            sb.AppendLine($"{GetIndent(indentSize)}{groupKey}:");

            var countValue = group.Value.Count.ToString();
            sb.AppendLine($"{GetIndent(indentSize * 2)}count: {countValue}");
            sb.AppendLine($"{GetIndent(indentSize * 2)}items:");

            foreach (var row in group.Value)
            {
                sb.AppendLine($"{GetIndent(indentSize * 3)}-");

                for (int i = 0; i < input.Headers.Length && i < row.Length; i++)
                {
                    if (i == groupColumnIndex) continue;

                    var header = FormatYamlKey(input.Headers[i], namingConvention);
                    var value = ProcessValue(row[i], convertDataTypes, dateFormat, escapeSpecialChars);

                    if (value == null && !includeNullValues) continue;

                    var yamlStyle = DetermineYamlStyle(value?.ToString(), multilineColumns.Contains(i));
                    var formattedValue = FormatYamlValue(value, multilineColumns.Contains(i),
                        dateFormat, escapeSpecialChars, preserveQuotes);

                    switch (yamlStyle)
                    {
                        case YamlStyle.Literal:
                            sb.AppendLine($"{GetIndent(indentSize * 4)}{header}: |");
                            var literalLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                            foreach (var line in literalLines)
                            {
                                sb.AppendLine($"{GetIndent(indentSize * 5)}{line}");
                            }

                            break;

                        case YamlStyle.Folded:
                            sb.AppendLine($"{GetIndent(indentSize * 4)}{header}: >");
                            var foldedLines = value?.ToString()?.Split('\n') ?? new[] { string.Empty };
                            foreach (var line in foldedLines)
                            {
                                sb.AppendLine($"{GetIndent(indentSize * 5)}{line}");
                            }

                            break;

                        default:
                            sb.AppendLine($"{GetIndent(indentSize * 4)}{header}: {formattedValue}");
                            break;
                    }
                }
            }
        }
    }

    private HashSet<int> DetectMultilineColumns(CsvData input)
    {
        var result = new HashSet<int>();

        for (int i = 0; i < input.Headers.Length; i++)
        {
            foreach (var row in input.Rows)
            {
                if (i < row.Length && row[i]?.Contains('\n') == true)
                {
                    result.Add(i);
                    break;
                }
            }
        }

        return result;
    }

    private string DetectColumnDataType(CsvData input, string headerName)
    {
        var headerIndex = Array.IndexOf(input.Headers, headerName);
        if (headerIndex == -1) return "string";

        var sampleValues = input.Rows
            .Where(row => headerIndex < row.Length && !string.IsNullOrEmpty(row[headerIndex]))
            .Select(row => row[headerIndex])
            .Take(10)
            .ToList();

        if (!sampleValues.Any()) return "string";

        if (sampleValues.All(v => int.TryParse(v, out _))) return "integer";
        if (sampleValues.All(v => double.TryParse(v, out _))) return "number";
        if (sampleValues.All(v => bool.TryParse(v, out _))) return "boolean";
        if (sampleValues.All(v => DateTime.TryParse(v, out _))) return "datetime";

        return "string";
    }

    private object? ProcessValue(string? value, bool convertDataTypes, string? dateFormat, bool escapeSpecialChars)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        if (!convertDataTypes)
            return escapeSpecialChars ? EscapeYamlString(value) : value;

        if (!string.IsNullOrEmpty(dateFormat))
        {
            if (DateTime.TryParseExact(value, dateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var specificDateValue))
            {
                return specificDateValue;
            }
        }
        else if (DateTime.TryParse(value, out var genericDateValue))
        {
            return genericDateValue;
        }

        if (int.TryParse(value, out var intValue))
            return intValue;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
            return doubleValue;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            return decimalValue;

        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("NULL", StringComparison.Ordinal))
        {
            return null;
        }

        return escapeSpecialChars ? EscapeYamlString(value) : value;
    }

    private YamlStyle DetermineYamlStyle(string? value, bool isMultiline)
    {
        if (string.IsNullOrEmpty(value))
            return YamlStyle.Default;

        if (isMultiline)
        {
            if (value.Count(c => c == '\n') > 1)
                return YamlStyle.Literal;

            if (value.Length > 80)
                return YamlStyle.Folded;
        }

        return YamlStyle.Default;
    }

    private string FormatYamlKey(string key, YamlNamingConvention convention)
    {
        if (string.IsNullOrEmpty(key))
            return "field";

        return convention switch
        {
            YamlNamingConvention.CamelCase => ToCamelCase(key),
            YamlNamingConvention.SnakeCase => ToSnakeCase(key),
            YamlNamingConvention.KebabCase => ToKebabCase(key),
            YamlNamingConvention.LowerCase => key.ToLower(),
            _ => key
        };
    }

    private string FormatYamlValue(object? value, bool isMultiline, string? dateFormat,
        bool escapeSpecialChars, bool preserveQuotes)
    {
        if (value == null)
            return "null";

        if (value is DateTime dateValue)
        {
            return !string.IsNullOrEmpty(dateFormat)
                ? dateValue.ToString(dateFormat)
                : dateValue.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        if (value is bool boolValue)
            return boolValue.ToString().ToLower();

        if (value is int || value is double || value is decimal)
            return value.ToString() ?? "0";

        var stringValue = value.ToString() ?? string.Empty;

        if (preserveQuotes || NeedsQuoting(stringValue))
        {
            stringValue = $"\"{(escapeSpecialChars ? EscapeYamlString(stringValue) : stringValue)}\"";
        }
        else if (escapeSpecialChars)
        {
            stringValue = EscapeYamlString(stringValue);
        }

        return stringValue;
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return input;

        var result = words[0].ToLower();
        for (int i = 1; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }

        return result;
    }

    private string ToSnakeCase(string input)
    {
        return input.ToLower().Replace(" ", "_").Replace("-", "_");
    }

    private string ToKebabCase(string input)
    {
        return input.ToLower().Replace(" ", "-").Replace("_", "-");
    }

    private bool NeedsQuoting(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.Contains(":") ||
               value.Contains("#") ||
               value.Contains("'") ||
               value.Contains("\"") ||
               value.StartsWith("-") ||
               value.StartsWith("[") ||
               value.StartsWith("{") ||
               value == "null" ||
               value == "true" ||
               value == "false" ||
               value == "yes" ||
               value == "no" ||
               value == "on" ||
               value == "off" ||
               char.IsWhiteSpace(value[0]) ||
               char.IsWhiteSpace(value[^1]);
    }

    private string EscapeYamlString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private string GetIndent(int level)
    {
        return new string(' ', level);
    }
}