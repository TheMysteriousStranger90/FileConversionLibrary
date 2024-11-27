using System.Xml.Linq;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;

namespace FileConversionLibrary;

public class CsvToXmlConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string xmlOutputPath, char delimiter = ',')
    {
        try
        {
            Console.WriteLine($"Reading CSV file from: {csvFilePath}");
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);

            var doc = new XDocument();
            var root = new XElement("root");
            doc.Add(root);
            
            foreach (var row in csvData.Rows)
            {
                var element = new XElement("element");
                
                for (var j = 0; j < csvData.Headers.Length; j++)
                {
                    var header = csvData.Headers[j]
                        .Trim()
                        .Replace(";", "")
                        .Replace(" ", "_")
                        .Replace("-", "_")
                        .Replace("/", "_");
                    
                    element.Add(new XElement(header, row[j]));
                }

                root.Add(element);
            }

            Console.WriteLine($"Saving XML file to: {xmlOutputPath}");
            doc.Save(xmlOutputPath);
            Console.WriteLine("XML file saved successfully.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"I/O error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}