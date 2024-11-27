using System.Xml;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FileConversionLibrary;

public class XmlToJsonConverter : IXmlConverter
{
    public async Task ConvertAsync(string xmlFilePath, string jsonOutputPath)
    {
        try
        {
            var (headers, rows) = await XmlHelperFile.ReadXmlAsync(xmlFilePath);

            var csvList = new List<Dictionary<string, string>>();
            foreach (var row in rows)
            {
                var rowData = new Dictionary<string, string>();
                for (var j = 0; j < headers.Length; j++)
                {
                    rowData.Add(headers[j], row[j]);
                }
                csvList.Add(rowData);
            }

            var json = JsonConvert.SerializeObject(csvList, Formatting.Indented);
            await File.WriteAllTextAsync(jsonOutputPath, json);
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
}