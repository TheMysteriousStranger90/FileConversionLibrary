using System.Xml;
using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FileConversionLibrary;

public class XmlToYamlConverter : IXmlConverter
{
    public async Task ConvertAsync(string xmlFilePath, string yamlOutputPath)
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

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(csvList);
            await File.WriteAllTextAsync(yamlOutputPath, yaml);
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