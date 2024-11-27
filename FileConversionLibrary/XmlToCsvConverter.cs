using System.Data;
using System.Xml;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary;

public class XmlToCsvConverter : IXmlConverter
{
    public async Task ConvertAsync(string xmlFilePath, string csvOutputPath)
    {
        try
        {
            var (headers, rows) = await XmlHelperFile.ReadXmlAsync(xmlFilePath);

            var lines = new List<string>
            {
                string.Join(",", headers.Select(QuoteValue))
            };

            foreach (var row in rows)
            {
                var line = string.Join(",", row.Select(QuoteValue));
                lines.Add(line);
            }

            await File.WriteAllLinesAsync(csvOutputPath, lines);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine($"File not found: {e.FileName}");
        }
        catch (XmlException e)
        {
            Console.WriteLine($"Invalid XML: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    private string QuoteValue(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            value = $"\"{value}\"";
        }

        return value;
    }
}