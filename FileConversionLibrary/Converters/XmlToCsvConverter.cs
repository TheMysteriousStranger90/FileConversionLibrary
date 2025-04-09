using System.Text;
using FileConversionLibrary.Interfaces;
using FileConversionLibrary.Models;

namespace FileConversionLibrary.Converters;

public class XmlToCsvConverter : IConverter<XmlData, string>
{
    public string Convert(XmlData input, object options = null)
    {
        if (input?.Headers == null || input.Rows == null)
        {
            throw new ArgumentException("Invalid XML data");
        }

        var delimiter = ',';
        if (options is char delimiterChar)
        {
            delimiter = delimiterChar;
        }
        else if (options is Dictionary<string, object> optionsDict &&
                 optionsDict.TryGetValue("delimiter", out var delimiterObj) &&
                 delimiterObj is char delimiterChar2)
        {
            delimiter = delimiterChar2;
        }

        var sb = new StringBuilder();

        sb.AppendLine(string.Join(delimiter.ToString(), input.Headers.Select(QuoteValue)));

        foreach (var row in input.Rows)
        {
            sb.AppendLine(string.Join(delimiter.ToString(), row.Select(QuoteValue)));
        }

        return sb.ToString();
    }

    private string QuoteValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            value = value.Replace("\"", "\"\"");
            value = $"\"{value}\"";
        }

        return value;
    }
}