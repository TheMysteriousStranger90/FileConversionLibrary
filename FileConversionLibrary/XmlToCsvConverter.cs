using System.Data;
using System.Xml;

namespace FileConversionLibrary;

public class XmlToCsvConverter
{
    public void ConvertXmlToCsv(string xmlFilePath, string csvOutputPath)
    {
        try
        {
            var xmlContent = File.ReadAllText(xmlFilePath);
            var xmlFile = new XmlDocument();
            xmlFile.LoadXml(xmlContent);

            var xmlReader = new XmlNodeReader(xmlFile);
            var dataSet = new DataSet();
            dataSet.ReadXml(xmlReader);

            foreach (DataTable table in dataSet.Tables)
            {
                WriteTableToCsv(table, csvOutputPath);
            }
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

    private void WriteTableToCsv(DataTable table, string outputPath)
    {
        var lines = new List<string>();

        var header = string.Join(",", table.Columns.Cast<DataColumn>().Select(column => QuoteValue(column.ColumnName)));
        lines.Add(header);

        foreach (DataRow row in table.Rows)
        {
            var line = string.Join(",", row.ItemArray.Select(value => QuoteValue(value.ToString())));
            lines.Add(line);
        }

        File.WriteAllLines(outputPath, lines);
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