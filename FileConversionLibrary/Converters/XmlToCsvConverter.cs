using System.Text;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class XmlToCsvConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object? options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        if (input.Headers.Length == 0)
        {
            throw new ArgumentException("No headers found in XML data");
        }

        var delimiter = ',';
        bool quoteValues = true;

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
        }

        var sb = new StringBuilder();

        var headerLine = string.Join(delimiter.ToString(),
            input.Headers.Select(h => quoteValues ? QuoteValue(h, delimiter) : h));
        sb.AppendLine(headerLine);

        foreach (var row in input.Rows)
        {
            var values = new string[input.Headers.Length];

            for (int i = 0; i < input.Headers.Length; i++)
            {
                if (i < row.Length)
                {
                    values[i] = quoteValues ? QuoteValue(row[i] ?? string.Empty, delimiter) : (row[i] ?? string.Empty);
                }
                else
                {
                    values[i] = string.Empty;
                }
            }

            sb.AppendLine(string.Join(delimiter.ToString(), values));
        }

        return sb.ToString();
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