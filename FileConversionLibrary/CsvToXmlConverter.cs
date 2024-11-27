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
                    var header = MakeValidXmlName(csvData.Headers[j]);
                    var value = row[j]?.Trim();
                    
                    element.Add(new XElement(header, value));
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
    
    private string MakeValidXmlName(string name)
    {
        var validName = new string(name.Where(char.IsLetterOrDigit).ToArray());
        
        if (char.IsDigit(validName.FirstOrDefault()))
        {
            validName = "_" + validName;
        }

        return validName;
    }
}