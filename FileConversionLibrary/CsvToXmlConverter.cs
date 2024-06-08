using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FileConversionLibrary;

public class CsvToXmlConverter
{
    public void ConvertCsvToXml(string csvFilePath, string xmlOutputPath, char delimiter = ',')
    {
        try
        {
            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"File not found: {csvFilePath}");
            }

            var csvContent = File.ReadAllLines(csvFilePath);
            var headers = ParseLine(csvContent[0], delimiter);

            var doc = new XDocument();
            var root = new XElement("root");
            doc.Add(root);

            for (var i = 1; i < csvContent.Length; i++)
            {
                var row = ParseLine(csvContent[i], delimiter);
                var element = new XElement("element");
                for (var j = 0; j < headers.Length; j++)
                {
                    element.Add(new XElement(headers[j], row[j]));
                }

                root.Add(element);
            }

            doc.Save(xmlOutputPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private string[] ParseLine(string line, char delimiter)
    {
        var pattern = $"{delimiter}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
        var regex = new Regex(pattern);
        var fields = regex.Split(line);

        for (var i = 0; i < fields.Length; i++)
        {
            fields[i] = fields[i].Trim('\"').Replace("\"\"", "\"");
        }

        return fields;
    }
}
