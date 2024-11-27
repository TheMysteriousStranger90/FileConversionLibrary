using FileConversionLibrary.Helpers;
using FileConversionLibrary.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FileConversionLibrary;

public class CsvToYamlConverter : ICsvConverter
{
    public async Task ConvertAsync(string csvFilePath, string yamlOutputPath, char delimiter = ',')
    {
        try
        {
            Console.WriteLine($"Reading CSV file from: {csvFilePath}");
            var csvData = await CsvHelperFile.ReadCsvAsync(csvFilePath, delimiter);

            var csvList = new List<Dictionary<string, string>>();

            foreach (var row in csvData.Rows)
            {
                var rowData = new Dictionary<string, string>();
                for (var j = 0; j < csvData.Headers.Length; j++)
                {
                    var header = CsvHelperFile.MakeValidYamlKey(csvData.Headers[j].Trim());
                    var value = row[j]?.Trim();
                    
                    if (!rowData.ContainsKey(header))
                    {
                        rowData.Add(header, value);
                    }
                }
                csvList.Add(rowData);
            }
            
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).WithIndentedSequences()
                .Build();

            var yaml = serializer.Serialize(csvList);

            Console.WriteLine($"Saving YAML file to: {yamlOutputPath}");
            await File.WriteAllTextAsync(yamlOutputPath, yaml);
            Console.WriteLine("YAML file saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}