using System.Text;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

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

    public string Convert(CsvData input, object? options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid CSV data");
        }

        var sb = new StringBuilder();
        sb.AppendLine("---");
        
        var multilineColumns = DetectMultilineColumns(input);

        foreach (var row in input.Rows)
        {
            sb.AppendLine("-");

            for (int i = 0; i < input.Headers.Length && i < row.Length; i++)
            {
                var header = FormatYamlKey(input.Headers[i]);
                var value = row[i] ?? string.Empty;

                YamlStyle style = DetermineYamlStyle(value, multilineColumns.Contains(i));
                
                switch (style)
                {
                    case YamlStyle.Literal:
                        sb.AppendLine($"    {header}: |");
                        foreach (var line in value.Split('\n'))
                        {
                            sb.AppendLine($"      {line}");
                        }
                        break;
                        
                    case YamlStyle.Folded:
                        sb.AppendLine($"    {header}: >");
                        foreach (var line in value.Split('\n'))
                        {
                            sb.AppendLine($"      {line}");
                        }
                        break;
                        
                    default:
                        if (NeedsQuoting(value))
                        {
                            value = $"\"{EscapeYamlString(value)}\"";
                        }
                        sb.AppendLine($"    {header}: {value}");
                        break;
                }
            }
        }

        return sb.ToString();
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

    private YamlStyle DetermineYamlStyle(string value, bool isMultiline)
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

    private string FormatYamlKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "field";
        
        if (!key.Contains(" ") && !key.Contains("-") && 
            !key.Contains(":") && !key.Contains("."))
            return key;
        
        return key.ToLower().Replace(" ", "_").Replace("-", "_");
    }

    private bool NeedsQuoting(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        
        return value.Contains(":") || 
               value.Contains("#") || 
               value.Contains("'") ||
               value.StartsWith("-") ||
               value == "null" || 
               value == "true" || 
               value == "false" ||
               (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]));
    }

    private string EscapeYamlString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
            
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r");
    }
}